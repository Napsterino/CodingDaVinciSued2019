using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;


namespace Prototype.NetworkLobby
{
#pragma warning disable 618
    public class LobbyManager : NetworkLobbyManager
#pragma warning restore 618
    {
        static public LobbyManager s_Singleton;

        [Header("Unity UI Lobby")]
        [Header("UI Reference")]
        public LobbyTopPanel topPanel;

        public RectTransform mainMenuPanel;
        public RectTransform lobbyPanel;

        public GameObject addPlayerButton;

        protected RectTransform currentPanel;

        public Button backButton;

        //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
        //of players, so that even client know how many player there is.
        [HideInInspector]
        public int _playerNumber = 0;

        protected LobbyHook _lobbyHooks;

        void Start()
        {
            s_Singleton = this;
            _lobbyHooks = GetComponent<Prototype.NetworkLobby.LobbyHook>();
            currentPanel = mainMenuPanel;

            backButton.gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;

            DontDestroyOnLoad(gameObject);
        }

#pragma warning disable 618
        public override void OnLobbyClientSceneChanged(NetworkConnection conn)
#pragma warning restore 618
        {
            if (SceneManager.GetSceneAt(0).name == lobbyScene)
            {
                if (topPanel.isInGame)
                {
                    ChangeTo(lobbyPanel);
                    if (conn.playerControllers[0].unetView.isClient)
                    {
                        backDelegate = StopHostClbk;
                    }
                    else
                    {
                        backDelegate = StopClientClbk;
                    }
                }
                else
                {
                    ChangeTo(mainMenuPanel);
                }

                topPanel.ToggleVisibility(true);
                topPanel.isInGame = false;
            }
            else
            {
                ChangeTo(null);

                Destroy(GameObject.Find("MainMenuUI(Clone)"));

                //backDelegate = StopGameClbk;
                topPanel.isInGame = true;
                topPanel.ToggleVisibility(false);
                GetComponent<Image>().enabled = false;
            }
        }

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }

            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }

            currentPanel = newPanel;

            if (currentPanel != mainMenuPanel)
            {
                backButton.gameObject.SetActive(true);
            }
            else
            {
                backButton.gameObject.SetActive(false);
                GetComponent<Image>().enabled = true;
            }
        }

        public delegate void BackButtonDelegate();
        public BackButtonDelegate backDelegate;
        public void GoBackButton()
        {
            backDelegate();
            topPanel.isInGame = false;
        }

        public void QuitButton()
        {
            Application.Quit();
        }

        // ----------------- Server management

        public void AddLocalPlayer()
        {
            TryToAddPlayer();
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            player.RemovePlayer();
        }

        public void SimpleBackClbk()
        {
            ChangeTo(mainMenuPanel);
        }

        public void StopHostClbk()
        {
            StopHost();
            ChangeTo(mainMenuPanel);
            GetComponent<Image>().enabled = true;
        }

        public void StopClientClbk()
        {
            StopClient();
            ChangeTo(mainMenuPanel);
            GetComponent<Image>().enabled = true;
        }

        public void StopServerClbk()
        {
            StopServer();
            ChangeTo(mainMenuPanel);
            GetComponent<Image>().enabled = true;
        }

        //===================

        public override void OnStartHost()
        {
            base.OnStartHost();

            ChangeTo(lobbyPanel);
            backDelegate = StopHostClbk;
        }

        //allow to handle the (+) button to add/remove player
        public void OnPlayersNumberModified(int count)
        {
            _playerNumber += count;

            int localPlayerCount = 0;
#pragma warning disable 618
            foreach (PlayerController p in ClientScene.localPlayers)
#pragma warning restore 618
                localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

            addPlayerButton.SetActive(localPlayerCount < maxPlayersPerConnection && _playerNumber < maxPlayers);
        }

        // ----------------- Server callbacks ------------------

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            //This hook allows you to apply state data from the lobby-player to the game-player
            //just subclass "LobbyHook" and add it to the lobby object.

            if (_lobbyHooks)
                _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

            return true;
        }

        // ----------------- Client callbacks ------------------

#pragma warning disable 618
        public override void OnClientConnect(NetworkConnection conn)
#pragma warning restore 618
        {
            base.OnClientConnect(conn);

#pragma warning disable 618
            if (!NetworkServer.active)
#pragma warning restore 618
            {//only to do on pure client (not self hosting client)
                ChangeTo(lobbyPanel);
                backDelegate = StopClientClbk;
            }
        }


#pragma warning disable 618
        public override void OnClientDisconnect(NetworkConnection conn)
#pragma warning restore 618
        {
            base.OnClientDisconnect(conn);
            ChangeTo(mainMenuPanel);
        }

#pragma warning disable 618
        public override void OnClientError(NetworkConnection conn, int errorCode)
#pragma warning restore 618
        {
            ChangeTo(mainMenuPanel);
        }
    }
}
