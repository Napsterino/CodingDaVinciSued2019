using Prototype.NetworkLobby;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace cdv
{
    public class MainUIRoot : MonoBehaviour
    {

        public static MainUIRoot Instance { get; private set; }

        public string TitleScene;

        public MainPanelController MainPanel;
        public TerritoryContextMenuController ContextMenu;
        public BuildingSelectorController BuildingMenu;

        public DiplomacyMenuController Diplomacy;
        public TradingMenuController Trading;

        public ActiveEffectsPanelManager EffectsPanel;

        public Resources ResourcesPanel;
        public VictoryPoints VictoryPointsPanel;
        public GameOver GameOverPanel;

        public IconAmountFieldController CurrentRound;

        private void Awake()
        {
            Instance = this;
#if NEW_GUI
            HideAll();
#else
            gameObject.SetActive(false);
#endif
        }

        [Serializable]
        public class Resources
        {
            public IconAmountFieldController Construction;
            public IconAmountFieldController Technology;
            public IconAmountFieldController Influence;
        }

        [Serializable]
        public class VictoryPoints
        {
            public IconAmountFieldController Science;
            public IconAmountFieldController Territory;
            public IconAmountFieldController Economy;
            public IconAmountFieldController Culture;
            public IconAmountFieldController TotalVictoryPoints;
        }

        [Serializable]
        public class GameOver
        {
            public GameObject Panel;
            public Text GameOverText;
        }

        // TODO Fabian: Call this method once every time the local player begins a new turn
        public void SetupUITurnBegin(bool enableDiplomacy, IList<Player> playersAtWarWith)
        {
            MainPanel.Diplomacy.interactable = enableDiplomacy;
            if ((playersAtWarWith != null) && playersAtWarWith.Count > 0)
            {
                if (playersAtWarWith.Count == 2)
                {
                    Diplomacy.DisplayDestroyResourcesView(playersAtWarWith[0], () =>
                    {
                        Diplomacy.DisplayDestroyResourcesView(playersAtWarWith[1], () =>
                        {
                            MainPanel.gameObject.SetActive(true);
                            Player.LocalPlayer.ProcessGUINextFrame();
                        });
                    });
                }
                else
                {
                    Diplomacy.DisplayDestroyResourcesView(playersAtWarWith[0], () =>
                    {
                        MainPanel.gameObject.SetActive(true);
                        Player.LocalPlayer.ProcessGUINextFrame();
                    });
                }
            }
            else
            {
                MainPanel.gameObject.SetActive(true);
                Player.LocalPlayer.ProcessGUINextFrame();
            }
        }

        public void CheckHideAll()
        {
            Player LocalPlayer = Player.LocalPlayer;

            MainPanel.Hide();
            ContextMenu.HideContextMenu();
            if (!LocalPlayer.ShowBuildingOverview)
            {
                BuildingMenu.Hide();
            }
            if (!LocalPlayer.ShowDiplomacy)
            {
                Diplomacy.Hide();
            }
            if (!LocalPlayer.ShowTrading)
            {
                Trading.Hide();
            }
        }

        public void HideAll()
        {
            Player LocalPlayer = Player.LocalPlayer;
            if (LocalPlayer != null)
            {
                LocalPlayer.ShowDiplomacy = false;
                LocalPlayer.ShowBuildingOverview = false;
                LocalPlayer.ShowTrading = false;
            }

            MainPanel.Hide();
            ContextMenu.HideContextMenu();
            BuildingMenu.Hide();
            Diplomacy.Hide();
            Trading.Hide();
        }

        // TODO Fabian: Call this method once when the local player ends a turn
        public void EndTurn()
        {
            HideAll();
            Player.LocalPlayer.ProcessGUINextFrame();
            Player.LocalPlayer.SelectedRegion?.Deselect();
            Player.LocalPlayer.SelectedRegion = null;
            Player.LocalPlayer.CmdEndTurn();
        }

        public void SetGameOver(string text)
        {
            GameOverPanel.Panel.SetActive(true);
            GameOverPanel.GameOverText.text = text;
        }

#pragma warning disable 618
        public void OnExitButton()
        {
            LobbyManager.s_Singleton.SendReturnToLobby();

            //NetworkManager.singleton.StopClient();
            //NetworkManager.singleton.StopHost();

            //NetworkLobbyManager.singleton.StopClient();
            //NetworkLobbyManager.singleton.StopServer();

            //NetworkServer.DisconnectAll();

            //StartCoroutine(ExitDelay());

        }

        IEnumerator ExitDelay()
        {
            yield return new WaitForSeconds(0.1f);//attends un peu
            Destroy(NetworkLobbyManager.singleton.gameObject);

            yield return new WaitForSeconds(0.1f);//attends un peu

            SceneManager.LoadScene(TitleScene);
        }
#pragma warning restore 618
    }
}