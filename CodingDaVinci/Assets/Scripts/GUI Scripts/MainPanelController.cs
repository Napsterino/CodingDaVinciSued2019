using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class MainPanelController : MonoBehaviour
    {
        public Button Diplomacy;
        public Button Trading;
        public Button Buildings;
        public Button TechnolodgyCard;
        public Button EndTurn;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void DisableAllPanels()
        {
            Player localPlayer = Player.LocalPlayer;
            localPlayer.ShowBuildingOverview = false;
            localPlayer.ShowDiplomacy = false;
            localPlayer.ShowTrading = false;
        }

        public void OnDiplomacy()
        {
            Player.LocalPlayer.ShowDiplomacy = !Player.LocalPlayer.ShowDiplomacy;
            if (Player.LocalPlayer.ShowDiplomacy)
            {
                DisableAllPanels();
                Player.LocalPlayer.ShowDiplomacy = true;
                MainUIRoot.Instance.Diplomacy.DisplayPlayerSelection();
            }
        }

        public void OnTrading()
        {
            Player.LocalPlayer.ShowTrading = !Player.LocalPlayer.ShowTrading;
            if (Player.LocalPlayer.ShowTrading)
            {
                DisableAllPanels();
                Player.LocalPlayer.ShowTrading = true;
                MainUIRoot.Instance.Trading.SelectMain();
            }
        }

        public void OnBuildings()
        {
            Player.LocalPlayer.ShowBuildingOverview = !Player.LocalPlayer.ShowBuildingOverview;
            if (Player.LocalPlayer.ShowBuildingOverview)
            {
                DisableAllPanels();
                Player.LocalPlayer.ShowBuildingOverview = true;
            }
        }

        public void OnTechCard()
        {
            Player.LocalPlayer.TryBuyTechnologyCard();
        }

        public void OnEndTurn()
        {
            MainUIRoot.Instance.EndTurn();
        }
    }
}