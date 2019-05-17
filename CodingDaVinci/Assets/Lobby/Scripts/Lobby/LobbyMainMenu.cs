using UnityEngine;
using UnityEngine.UI;

namespace Prototype.NetworkLobby
{
    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour
    {
        public LobbyManager lobbyManager;

        public RectTransform lobbyPanel;
        public GameObject subpanel_DirectPlay;
        public GameObject subpanel_About;
        public GameObject subpanel_Help;
        public GameObject subpanel_Settings;

        public InputField ipInput;

        public void OnEnable()
        {
            lobbyManager.topPanel.ToggleVisibility(true);

            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(onEndEditIP);

        }

        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            lobbyManager.ChangeTo(lobbyPanel);

            lobbyManager.networkAddress = ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;

        }

        void onEndEditIP(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickJoin();
            }
        }

        public void OnClickAbout()
        {
            SetSubPanelActive(subpanel_About);
        }

        public void OnClickHelp()
        {
            SetSubPanelActive(subpanel_Help);
        }

        public void OnClickSettings()
        {
            SetSubPanelActive(subpanel_Settings);
        }

        public void SetSubPanelActive(GameObject panel)
        {
            if (panel != subpanel_DirectPlay)
            {
                subpanel_DirectPlay?.SetActive(false);
                lobbyManager.backDelegate = () => { SetSubPanelActive(subpanel_DirectPlay); };
                lobbyManager.backButton.gameObject.SetActive(true);
            }
            else
            {
                lobbyManager.backButton.gameObject.SetActive(false);
            }
            if (panel != subpanel_About)
            {
                subpanel_About?.SetActive(false);
            }
            if (panel != subpanel_Help)
            {
                subpanel_Help?.SetActive(false);
            }
            if (panel != subpanel_Settings)
            {
                subpanel_Settings?.SetActive(false);
            }
            panel?.SetActive(true);
        }
    }
}
