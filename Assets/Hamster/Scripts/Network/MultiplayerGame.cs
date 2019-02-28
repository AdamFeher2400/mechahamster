﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Hamster;
using Hamster.States;
/*
 * Note: Although it would be cleaner if this was a separate GameObject than NetworkManager, the way Unity works makes that more trouble than it's worth. Because the NetworkManager
 * is set to DontDestroyOnLoad, it gets put into a separate kind of scene than the other objects. They are separated such that one cannot refer to the other. Since MultiplayerGame needs to
 * refer to NetworkManager, that makes that structure a no-go. Thus, it's simply placed as a component onto the same Component as CustomNetworkManager. It's unclear what ramifications this may
 * have later down the road. But for now, it works.
 */
public class MultiplayerGame : MonoBehaviour
{
    //  game constants
    const int kMaxPlayers = 4;

    static public MultiplayerGame s_instance;
    static public MultiplayerGame instance { get { return s_instance; } }

    //  config file for multiplayer game
    public const string kConfigJsonBasename = "MHConfig";
    public const string kConfigJson = kConfigJsonBasename + ".json";

    public bool autoStartServer;    //  if we want the server to automatically start. This is mostly for debugging because Unity doesn't play nice with Start() and Network scenes!
    public NetworkManager manager;  //  developer should fill this field in Unity inspector.
    public JsonStartupConfig config;
    public ConnectionConfig connConfig; //  deprecated

    //  warning: These should be private, but are public for convenience for debugging and displaying state info. Use with appropriate caution. Try to only do read operations and not write operations on these states.
    public StateManager clientStateManager = new StateManager();    //  this is for the client. The server is separated because "Host" can have both on the same machine!
    public StateManager serverStateManager = new StateManager();    //  our statemachine that is separate from the MainGame state machine for single player.

    public int numPlayers = 1;  //  this is the number of players who can join. Autostart after this number is reached on the server
    public int defaultLevelIdx = 0;
    public int startingLevel;
    public float[] playerFinishTimes = new float[kMaxPlayers];  //  where we record the finish times of the players.
    public NetworkConnection[] networkConnections = new NetworkConnection[kMaxPlayers];

    string serverAddress;
    string serverPort;

    //  for debugging
    public string curState;
    public void ReadConfig()
    {
        MultiplayerGame mpgame = this;
        mpgame.config = FindObjectOfType<JsonStartupConfig>();

        if (mpgame.config != null)
        {
            if (!mpgame.config.isConfigLoaded)
            {  //  strange, this should have already been loaded. But Unity timing for Start is weird, so we'll just load it anyway.
                mpgame.config.ReadJsonStartupConfig();
            }
            mpgame.serverAddress = mpgame.config.startupConfig.serverIP;
            mpgame.serverPort = mpgame.config.startupConfig.serverPort;
            mpgame.startingLevel = mpgame.config.startupConfig.startingLevel;
        }
    }

    //  typical unity stuff below
    private void Awake()
    {
        if (s_instance == null)
            s_instance = this;
        DontDestroyOnLoad(this.gameObject); //  because NetworkManager has been set to DontDestroyOnLoad, it will be in a separate scene hierarchy/memory segment that cannot interact with this. Thus we must be in the same "zone" as the NetworkManager! Ugh, Unity!
        for (int ii = 0; ii < kMaxPlayers; ii++)
        {
            playerFinishTimes[ii] = -1.0f;  //  initialize to negative time.
        }
    }

    void EnterServerStartupState()
    {
        ServerEnterMultiPlayerState<Hamster.States.ServerStartup>();
    }

    public void ClientEnterMultiPlayerState<T>(int mode = 0) where T : Hamster.States.BaseState, new()
    {
        ClientEnterMultiPlayerState<Hamster.States.ServerStartup>();
    }
    public void ServerEnterMultiPlayerState<T>(int mode=0) where T : Hamster.States.BaseState, new()
    {
        MultiplayerGame.EnterMultiPlayerState<T>(serverStateManager, mode);
    }

    //  this is private. Use MultiplayerGame.instance.ClientEnterMultiPlayerState or MultiplayerGame.instance.ClientEnterMultiPlayerState to make explicit whether server or client FSM is affected.
    static private void EnterMultiPlayerState<T>(StateManager stateManager, int mode=0) where T : Hamster.States.BaseState, new()
    {
        Hamster.States.BaseState state = new T();
        stateManager.PushState(state);
    }


    public void OnClientConnect(NetworkConnection conn)
    {
        networkConnections[0] = conn;   //  we're on the client, so we have only ONE connection to the server.
        //Hamster.States.ClientConnected state = new Hamster.States.ClientConnected();   //  create new state for FSM that will let us force the starting level.
        //stateManager.PushState(state);
        //  this replaces the above with a templated version.
        MultiplayerGame.EnterMultiPlayerState<Hamster.States.ClientConnected>(MultiplayerGame.instance.clientStateManager);

    }
    // Start is called before the first frame update
    void Start()
    {
        if (autoStartServer)
        {
            EnterServerStartupState();
        }
    }


    // Update is called once per frame
    void Update()
    {
        curState = clientStateManager.CurrentState().ToString();
        if (clientStateManager.CurrentState().ToString() == "Hamster.States.BaseState")  //  we haven't started anything yet.
        {
            if (manager != null && manager.isNetworkActive)
            {
                if (NetworkServer.active)
                {
                    ServerListenForClients listenState = new ServerListenForClients();
                    clientStateManager.PushState(listenState);
                }
            }
        }
        clientStateManager.Update();
    }
    // Pass through to allow states to have their own GUI.
    void OnGUI()
    {
        clientStateManager.OnGUI();
    }

}// class MultiPlayerGame