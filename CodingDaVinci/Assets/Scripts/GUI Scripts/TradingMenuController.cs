using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class TradingMenuController : MonoBehaviour
    {
        [Header("Unity UI")]
        public GameObject Panel;
        public GameObject MainSelectionView;
        public GameObject CreateOfferView;
        public GameObject OfferListView;
        public GameObject ResourcesSelectorView;
        public Text ResourcesSelectorText;
        public GameObject TradeOfferListRoot;
        public Text CloseBackButton;
        [Header("Controllers")]
        public TradeSelectorController OfferingSelector;
        public TradeSelectorController RequestingSelector;
        [Header("Prefab")]
        public GameObject TradeOfferListingPrefab;

        private bool buttonClosesPanel = true;

        public void Show()
        {
            Panel.SetActive(true);
            buttonClosesPanel = true;
        }

        public void Hide()
        {
            Panel.SetActive(false);
        }

        public void DisableAllViews()
        {
            CreateOfferView.SetActive(false);
            OfferListView.SetActive(false);
            MainSelectionView.SetActive(false);
            ResourcesSelectorView.SetActive(false);
        }

        public void SelectMain()
        {
            Show();
            DisableAllViews();
            MainSelectionView.SetActive(true);
            SetCloseBackButtonToCloseMode();
        }

        public void SelectCreateOffer()
        {
            Show();
            DisableAllViews();
            CreateOfferView.SetActive(true);
            SetCloseBackButtonToBackMode();
            Resources playerResources = Player.LocalPlayer.Resources;
            OfferingSelector.ResetTypeAndAmount(playerResources.ConstructionMaterial, playerResources.Technology, playerResources.Power);
            RequestingSelector.ResetTypeAndAmount(9, 9, 9);
        }

        public void SelectOfferList()
        {
            Show();
            DisableAllViews();
            OfferListView.SetActive(true);
            SetCloseBackButtonToBackMode();
            foreach (Transform child in TradeOfferListRoot.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (TradingOffer offer in GameManager.Instance.StockExchange.Offers)
            {
                int index = Player.LocalPlayer.Diplomacy.PlayerRelationships.Find(offer.Owner);
                bool isInAlliance = index != -1 &&
                    (Player.LocalPlayer.Diplomacy.PlayerRelationships[index].State == RelationshipState.TradingAgreement ||
                     Player.LocalPlayer.Diplomacy.PlayerRelationships[index].State == RelationshipState.Alliance);
                bool canAccept = Player.LocalPlayer.Resources.HasResources(offer.RequestedAsResourceInfoList);
                TradeOfferListingController.Instantiate(TradeOfferListingPrefab, TradeOfferListRoot, offer, canAccept && isInAlliance);
            }
        }

        private int Amount;

        public void ShowResourcesSelectorView(string text, int amount)
        {
            Show();
            DisableAllViews();
            ResourcesSelectorView.SetActive(true);
            ResourcesSelectorText.text = text;
            Amount = amount;
            SetCloseBackButtonToCloseMode();
        }

        public void OnAddConstruction()
        {
            Player.LocalPlayer.Resources.AddConstructionMaterial(Amount, false);
            Player.LocalPlayer.State = PlayerState.CurrentPlayer;
        }

        public void OnAddTechnology()
        {
            Player.LocalPlayer.Resources.AddTechnology(Amount, false);
            Player.LocalPlayer.State = PlayerState.CurrentPlayer;
        }

        public void OnAddInfluence()
        {
            Player.LocalPlayer.Resources.AddPower(Amount, false);
            Player.LocalPlayer.State = PlayerState.CurrentPlayer;
        }

        public void OnCloseBackButtonPressed()
        {
            if (buttonClosesPanel)
            {
                Hide();
            }
            else
            {
                SelectMain();
            }
        }

        public void CreateTradeOffer()
        {
            ResourceInfo costs = new ResourceInfo() { Type = OfferingSelector.SelectedType, Amount = OfferingSelector.Amount };
            if (Player.LocalPlayer.Resources.HasResources(costs))
            {
                Player.LocalPlayer.CmdPostTradingOffer(OfferingSelector.SelectedType, OfferingSelector.Amount.ToString(),
                    RequestingSelector.SelectedType, RequestingSelector.Amount.ToString());
                Hide();
            }
        }

        private void SetCloseBackButtonToCloseMode()
        {
            CloseBackButton.text = "Schließen";
            buttonClosesPanel = true;
        }

        private void SetCloseBackButtonToBackMode()
        {
            CloseBackButton.text = "Zurück";
            buttonClosesPanel = false;
        }
    }
}
