using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  this is where the players can simply join at any time and run around the map doing whatever they want until 4 players are reached and OpenMatch is triggered
    public class ServerPreOpenMatchGamePlay : BaseState
    {
        public NetworkManager manager;
        public CustomNetworkManagerHUD hud;
        int openMatchStartThreshold = MultiplayerGame.kOpenMatchThreshold;    //  automatically start OpenMatch after this number of players
        int     curNumPlayers;

        override public void Initialize()
        {
            string msg = "ServerPreOpenMatchGamePlay.Initialize";
            Debug.Log(msg);
            if (manager == null)
            {
                manager = MultiplayerGame.instance.manager;
                //  hmm this seems to fail hard.
                // manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();//GetComponent<NetworkManager>();
                if (manager == null)
                {
                    GameObject nm = GameObject.FindGameObjectWithTag("NetworkManager");
                    if (nm == null)
                    {
                    }
                    else
                    {
                        manager = nm.GetComponent<NetworkManager>();
                    }
                }
                if (manager != null)
                {
                    curNumPlayers = manager.numPlayers;
                    hud = manager.GetComponent<CustomNetworkManagerHUD>();
                }

            }
            if (hud != null)
                hud.showClientDebugInfoMessage(msg);
        }
        override public void OnGUI()
        {
            //Debug.Log("ServerPreOpenMatchGamePlay.OnGUI");
            if (hud != null)
            {
                //hud.scaledTextBox("curNumPlayers=" + curNumPlayers.ToString());
            }
        }

        // both Lobby and OpenMatch gameplay takes place here and just spins in this update loop until something happens.
        //  for lobby, when 4 players are found, swap to state: ServerOpenMatchStart
        //  for agones (i.e. OpenMatch cloud game), simply shut down when everyone leaves when the match is over: ServerEndPreGameplay
        override public void Update()
        {
            //Debug.Log("ServerPreOpenMatchGamePlay.Update");
            curNumPlayers = manager.numPlayers;

            if (curNumPlayers >= openMatchStartThreshold && MultiplayerGame.instance.agones == null)
            {
                //  fire off the OpenMatchState!
                MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerOpenMatchStart>();
            }
            else if (curNumPlayers <= 0)
            {
                //  No players. End of Match
                MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerEndPreGameplay>();
            }
        }
    }
}   //  Hamster.States