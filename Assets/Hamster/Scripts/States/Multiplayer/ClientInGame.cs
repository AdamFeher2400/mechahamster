using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  This client is playing the game now.
    public class ClientInGame : BaseState
    {
        public NetworkManager manager;
        public CustomNetworkManagerHUD hud;
        private string myDebugMsg;
        bool isClientSceneAddPlayerCalled = false; //  must have called ClientScene.AddPlayer in one way or another

        void GetPointers()
        {
            if (manager == null || hud == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();
                if (manager != null)
                {
                    hud = manager.GetComponent<CustomNetworkManagerHUD>();
                }
                else
                {
                    Debug.LogError("ClientInGame.GetPointers could not find NetworkManager!\n");
                }
            }

        }
        override public void Initialize()
        {
            GetPointers();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void OnGUI()
        {
            hud.scaledTextBox(myDebugMsg);
        }
        // Update is called once per frame
        public override void Update()
        {
            //Debug.Log("ClientInGame.Update() nplayers=" + manager.numPlayers.ToString());
            //Debug.Log("ClientInGame.Update() NetworkClient.allClients.Count=" + NetworkClient.allClients.Count.ToString());
            //Debug.Log("ClientInGame.Update() NetworkClient.allClients[0].isConnected=" + NetworkClient.allClients[0].isConnected.ToString());
            //  note: on the client, we can't actually keep track of how many players are in the game!
            if (hud != null)
            {
                myDebugMsg = "ClientInGame: nPlr=" + manager.numPlayers.ToString() + " nClients=" + NetworkClient.allClients.Count.ToString() + "\n\tNetClient.active=" + NetworkClient.active.ToString();
            }
            else
            {
                GetPointers();
            }
            if (!NetworkClient.active)  //  our connection is gone, so let's finish all shutdown.
            {
                Debug.LogError("ClientInGame shutdown!\n");
                NetworkClient.ShutdownAll();    //  
                MultiplayerGame.instance.ClientEnterMultiPlayerState<Hamster.States.ServerEndPreGameplay>();

            }
            else if (manager.numPlayers <= 0 && (NetworkClient.allClients.Count > 0) && !NetworkClient.allClients[0].isConnected)
            {
                NetworkClient.ShutdownAll();
                MultiplayerGame.instance.ClientEnterMultiPlayerState<Hamster.States.ServerEndPreGameplay>();
            }
        }
    }
}   //  Hamster.States