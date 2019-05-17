using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class TradeOfferListingController : MonoBehaviour
    {
        public Text Display;
        public Button AcceptButton;
        public Text ButtonText;
        private TradingOffer source;

        [HideInInspector]
        public bool SelfOwn = false;

        public void OnButton()
        {
            if (SelfOwn)
            {
                Player.LocalPlayer.CmdRemoveTradingOffer(source);
                Destroy(gameObject);
            }
            else
            {
                Player.LocalPlayer.CmdAcceptTradingOffer(source);
                Destroy(gameObject);
            }
        }

        public static void Instantiate(GameObject Prefab, GameObject parent, TradingOffer source, bool canAccept)
        {
            TradeOfferListingController tradeOfferRepresentation =  Instantiate(Prefab, parent.transform).GetComponent<TradeOfferListingController>();
            Player sourcePlayer = GameManager.Instance.GetPlayerOfId(source.Owner);
            string sourcePlayerName;
            if (sourcePlayer)
            {
                sourcePlayerName = sourcePlayer.ToString();
            }
            else
            {
                sourcePlayerName = source.Owner.ToString();
            }
            tradeOfferRepresentation.Display.text = $"Spieler {sourcePlayerName}: Gebe {source.OfferedAmount} x {Translationhelper.Get(source.OfferedResource.ToString())} für {source.RequestedAmount} x {Translationhelper.Get(source.RequestedResource.ToString())}";
            if (source.Owner == Player.LocalPlayer.netId)
            {
                tradeOfferRepresentation.ButtonText.text = "Zurückziehen";
                tradeOfferRepresentation.SelfOwn = true;
            }
            else if (!canAccept)
            {
                tradeOfferRepresentation.AcceptButton.interactable = false;
            }
            tradeOfferRepresentation.source = source;
        }
    }
}
