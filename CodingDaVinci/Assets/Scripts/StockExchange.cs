using UnityEngine;
using UnityEngine.Networking;

namespace cdv
{
    public struct TradingOffer
    {
#pragma warning disable 618
        public NetworkInstanceId Owner;
#pragma warning restore 618
        public ResourceType OfferedResource;
        public int OfferedAmount;
        public ResourceType RequestedResource;
        public int RequestedAmount;
    };

#pragma warning disable 618
    public class TradingOffers : SyncListStruct<TradingOffer> { }
#pragma warning restore 618

#pragma warning disable 618
    public sealed class StockExchange : NetworkBehaviour
#pragma warning restore 618
    {
        public TradingOffers Offers = new TradingOffers();
        public void Draw(Player player)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 200, 200, 400));
            if(ShowUI)
            {
                if(GUILayout.Button("Neues Angebot erstellen"))
                {
                    ShowUI = false;
                    ShowNewOfferScreen = true;
                }
                if(GUILayout.Button("Offene Angebote zeigen"))
                {
                    ShowOffers = true;
                    ShowUI = false;
                }
                if(GUILayout.Button("Handelsübersicht schließen"))
                {
                    ShowUI = false;
                }
            }
            else if(ShowNewOfferScreen)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Baustoffe"))
                {
                    OfferedResource = ResourceType.ConstructionMaterial;
                }
                if(GUILayout.Button("Einfluss"))
                {
                    OfferedResource = ResourceType.Power;
                }
                if(GUILayout.Button("Technologie"))
                {
                    OfferedResource = ResourceType.Technology;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Amount: ");
                AmountOffered = GUILayout.TextField(AmountOffered);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Baustoffe"))
                {
                    RequestedResource = ResourceType.ConstructionMaterial;
                }
                if(GUILayout.Button("Einfluss"))
                {
                    RequestedResource = ResourceType.Power;
                }
                if(GUILayout.Button("Technologie"))
                {
                    RequestedResource = ResourceType.Technology;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Amount: ");
                AmountRequested = GUILayout.TextField(AmountRequested);
                GUILayout.EndHorizontal();
                if(GUILayout.Button("Angebot erstellen"))
                {
                    player.CmdPostTradingOffer(OfferedResource, AmountOffered,
                        RequestedResource, AmountRequested);
                }
                if(GUILayout.Button("Abbrechen"))
                {
                    ShowNewOfferScreen = false;
                    ShowUI = true;
                }
            }
            else if(ShowOffers)
            {
                foreach(var offer in Offers)
                {
                    if(GUILayout.Button($"{offer.Owner} möchte {offer.OfferedAmount} {offer.OfferedResource}" +
                        $" gegen {offer.RequestedAmount} {offer.RequestedResource} tauschen"))
                    {
                        SelectedOffer = offer;
                    }
                }
                if(GUILayout.Button("Zurück"))
                {
                    ShowOffers = false;
                    ShowUI = true;
                }
            }
            else
            {
                if(GUILayout.Button("Handelsübersicht öffnen"))
                {
                    ShowUI = true;
                }
            }
            GUILayout.EndArea();

            if(SelectedOffer != null)
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200));
                if(SelectedOffer.Value.Owner == player.netId)
                {
                    if(GUILayout.Button("Angebot zurückziehen"))
                    {
                        player.CmdRemoveTradingOffer(SelectedOffer.Value);
                        SelectedOffer = null;
                    }
                    if(GUILayout.Button("Zurück"))
                    {
                        SelectedOffer = null;
                    }
                }
                else
                {
                    if(GUILayout.Button("Angebot annehmen"))
                    {
                        player.CmdAcceptTradingOffer(SelectedOffer.Value);
                        SelectedOffer = null;
                    }
                    if(GUILayout.Button("Zurück"))
                    {
                        SelectedOffer = null;
                    }
                }
                GUILayout.EndArea();
            }
        }

        #region Client Code
        private bool ShowUI = false;
        private bool ShowNewOfferScreen = false;
        private bool ShowOffers = false;
        private ResourceType OfferedResource = ResourceType.ConstructionMaterial;
        private string AmountOffered;
        private ResourceType RequestedResource = ResourceType.ConstructionMaterial;
        private string AmountRequested;
        private TradingOffer? SelectedOffer = null;
        #endregion

        #region Server Code
        #endregion
    }
}