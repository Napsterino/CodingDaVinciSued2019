using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace cdv
{
    public class DiplomacyMenuController : MonoBehaviour
    {
        [Header("Format Strings")]
        public string FormatStartWar = "Krieg gegen {0} erklären";
        public string FormatEndWar = "Mit {0} Frieden schließen";
        public string FormatOfferAlliance = "Ein Bündnis mit {0} schließen";
        public string FormatEndAlliance = "Das Bündnis mit {0} aufkündigen";
        public string FormatOfferTradingAlliance = "Mit {0} ein Handelsabkommen schließen";
        public string FormatEndTradingAlliance = "Das Handelsabkommen mit {0} verlassen";
        public string FormatDestroyResources = "Du befindest dich im Krieg mit {0}. Zerstöre nun zwei Resourcen des Gegners!";

        [Header("GUI Objects")]
        public GameObject Panel;
        public GameObject MainSelectorView;
        public GameObject PromptView;
        public GameObject PlayerSelectorView;
        public GameObject DestroyResourcesView;
        public GameObject ChangeRelationShipView;
        public Button ButtonStartWar;
        public Button ButtonEndWar;
        public Button ButtonOfferAlliance;
        public Button ButtonEndAlliance;
        public Button ButtonOfferTradingAlliance;
        public Button ButtonEndTradingAlliance;
        public Button ButtonDestroyConstructionMaterial;
        public Button ButtonDestroyTechnology;
        public Button ButtonAcceptPrompt;
        public Button ButtonDeclinePrompt;
        public Text TextPrompt;
        public Text TextDestroyResources;
        public Button ButtonDestroyInfluence;
        public Button RelationShipChangingDone;
        public Text RelationShipChangeText;

        public Transform PlayerListRoot;
        public Transform RelationShipListRoot;

        [Space]
        public GameObject PlayerPrefab;
        public GameObject RelationShipPrefab;

        private SimpleDelegate OnStartWarDelegate;
        private SimpleDelegate OnEndWarDelegate;
        private SimpleDelegate OnOfferAllianceDelegate;
        private SimpleDelegate OnEndAllianceDelegate;
        private SimpleDelegate OnOfferTradingAllianceDelegate;
        private SimpleDelegate OnEndTradingAllianceDelegate;

        public void EnableDiplomacy()
        {
            MainUIRoot.Instance.MainPanel.Diplomacy.interactable = true;
        }

        public void DisableDiplomacy()
        {
            Hide();
            MainUIRoot.Instance.MainPanel.Diplomacy.interactable = false;
            Player.LocalPlayer.ShowDiplomacy = false;
        }

        public void Hide()
        {
            Panel.SetActive(false);
        }

        public void Show()
        {
            Panel.SetActive(true);
        }

        public void DisableAllViews()
        {
            MainSelectorView.SetActive(false);
            PromptView.SetActive(false);
            PlayerSelectorView.SetActive(false);
            DestroyResourcesView.SetActive(false);
            ChangeRelationShipView.SetActive(false);
        }

        #region MainSelectView

        public void DisplayDiplomacyPanel(string selectedPlayerName, SimpleDelegate onStartWar = null, SimpleDelegate onEndWar = null, SimpleDelegate onOfferAlliance = null, SimpleDelegate onEndAlliance = null, SimpleDelegate onOfferTradingAlliance = null, SimpleDelegate onEndTradingAlliance = null)
        {
            Show();
            DisableAllViews();
            MainSelectorView.SetActive(true);

            if (onStartWar != null)
            {
                OnStartWarDelegate = onStartWar;
                UpdateButtonText(ButtonStartWar, selectedPlayerName, FormatStartWar);
            }
            else
            {
                OnStartWarDelegate = null;
                ButtonStartWar.gameObject.SetActive(false);
            }

            if (onEndWar != null)
            {
                OnEndWarDelegate = onEndWar;
                UpdateButtonText(ButtonEndWar, selectedPlayerName, FormatEndWar);
            }
            else
            {
                OnEndWarDelegate = null;
                ButtonEndWar.gameObject.SetActive(false);
            }

            if (onOfferAlliance != null)
            {
                OnOfferAllianceDelegate = onOfferAlliance;
                UpdateButtonText(ButtonOfferAlliance, selectedPlayerName, FormatOfferAlliance);
            }
            else
            {
                OnOfferAllianceDelegate = null;
                ButtonOfferAlliance.gameObject.SetActive(false);
            }

            if (onEndAlliance != null)
            {
                OnEndAllianceDelegate = onEndAlliance;
                UpdateButtonText(ButtonEndAlliance, selectedPlayerName, FormatEndAlliance);
            }
            else
            {
                OnEndAllianceDelegate = null;
                ButtonEndAlliance.gameObject.SetActive(false);
            }

            if (onOfferTradingAlliance != null)
            {
                OnOfferTradingAllianceDelegate = onOfferTradingAlliance;
                UpdateButtonText(ButtonOfferTradingAlliance, selectedPlayerName, FormatOfferTradingAlliance);
            }
            else
            {
                OnOfferTradingAllianceDelegate = null;
                ButtonOfferTradingAlliance.gameObject.SetActive(false);
            }

            if (onEndTradingAlliance != null)
            {
                OnEndTradingAllianceDelegate = onEndTradingAlliance;
                UpdateButtonText(ButtonEndTradingAlliance, selectedPlayerName, FormatEndTradingAlliance);
            }
            else
            {
                OnEndTradingAllianceDelegate = null;
                ButtonEndTradingAlliance.gameObject.SetActive(false);
            }
        }

        public void UpdateButtonText(Button button, string playerName, string format)
        {
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<Text>().text = string.Format(format, playerName);
        }

        public void OnStartWar()
        {
            Panel.SetActive(false);
            OnStartWarDelegate?.Invoke();
        }

        public void OnEndWar()
        {
            Panel.SetActive(false);
            OnEndWarDelegate?.Invoke();
        }

        public void OnOfferAlliance()
        {
            Panel.SetActive(false);
            OnOfferAllianceDelegate?.Invoke();
        }

        public void OnEndAlliance()
        {
            Panel.SetActive(false);
            OnEndAllianceDelegate?.Invoke();
        }

        public void OnOfferTradingAlliance()
        {
            Panel.SetActive(false);
            OnOfferTradingAllianceDelegate?.Invoke();
        }

        public void OnEndTradingAlliance()
        {
            Panel.SetActive(false);
            OnEndTradingAllianceDelegate?.Invoke();
        }

        #endregion
        #region Player Selection

        public void DisplayPlayerSelection()
        {
            if (!playersPopulated)
            {
                PopulatePlayers(GameManager.Instance.Players);
            }
            Show();
            DisableAllViews();
            PlayerSelectorView.SetActive(true);
            Player.LocalPlayer.ShowDiplomacy = true;
        }

        private bool playersPopulated = false;

        public void PopulatePlayers(IReadOnlyCollection<Player> players)
        {
            foreach (Player player in players)
            {
                if (!player.isLocalPlayer)
                {
                    GameObject instance = Instantiate(PlayerPrefab, PlayerListRoot);
                    var relay = instance.GetComponent<ButtonRelayWithPlayer>();
                    relay.AssociatedPlayer = player;
                    relay.OnClick.AddListener(OnPlayerButtonClicked);
                    instance.GetComponentInChildren<Text>().text = player.ToString();
                }
            }
            playersPopulated = true;
        }

        public void OnPlayerButtonClicked(Player player)
        {
            PlayerSelectorView.SetActive(false);
            Player.LocalPlayer.Diplomacy.SetupDiplomacyUI(player);
        }

        #endregion
        #region Destroying Resources

        private Player DestroyTarget;
        private SimpleDelegate OnDone;
        private int resourcesDestroyed;

        public void DisplayDestroyResourcesView(Player targetPlayer, SimpleDelegate onDone)
        {
            Show();
            DisableAllViews();
            DestroyResourcesView.SetActive(true);
            TextDestroyResources.text = string.Format(FormatDestroyResources, targetPlayer.PlayerName);
            DestroyResourcesView.SetActive(true);
            DestroyTarget = targetPlayer;
            OnDone = onDone;
            resourcesDestroyed = 0;
            ReevaluateResourceDestroyButtons();
        }

        public void ReevaluateResourceDestroyButtons()
        {
            if (DestroyTarget.Resources.ConstructionMaterial <= 0)
            {
                ButtonDestroyConstructionMaterial.interactable = false;
            }
            else
            {
                ButtonDestroyConstructionMaterial.interactable = true;
            }
            if (DestroyTarget.Resources.Technology <= 0)
            {
                ButtonDestroyTechnology.interactable = false;
            }
            else
            {
                ButtonDestroyTechnology.interactable = true;
            }
            if (DestroyTarget.Resources.Power <= 0)
            {
                ButtonDestroyInfluence.interactable = false;
            }
            else
            {
                ButtonDestroyInfluence.interactable = true;
            }
        }

        private void CheckResourcesDestroyed()
        {
            if (resourcesDestroyed == 1)
            {
                ReevaluateResourceDestroyButtons();
            }
            else
            {
                InvokeOnDone();
            }
        }

        public void OnDestroyConstructionMaterial()
        {
            Player.LocalPlayer.CmdRemoveResource(DestroyTarget.netId, ResourceType.ConstructionMaterial);
            resourcesDestroyed++;
            CheckResourcesDestroyed();
        }

        public void OnDestroyTechnology()
        {
            Player.LocalPlayer.CmdRemoveResource(DestroyTarget.netId, ResourceType.Technology);
            DestroyTarget.Resources.Technology--;
            resourcesDestroyed++;
            CheckResourcesDestroyed();
        }

        public void OnDestroyInfluence()
        {
            Player.LocalPlayer.CmdRemoveResource(DestroyTarget.netId, ResourceType.Power);
            resourcesDestroyed++;
            CheckResourcesDestroyed();
        }

        public void InvokeOnDone()
        {
            DestroyResourcesView.SetActive(false);
            MainSelectorView.SetActive(true);
            Player.LocalPlayer.CmdNextPlayerInWarWithOrEnd();
            OnDone?.Invoke();

        }

        #endregion
        #region Prompt

        private SimpleDelegate OnAccept;
        private SimpleDelegate OnDecline;

        public void DisplayPrompt(string promptText, SimpleDelegate onAccept, SimpleDelegate onDecline)
        {
            Show();
            DisableAllViews();
            PromptView.SetActive(true);
            TextPrompt.text = promptText;
            OnAccept = onAccept;
            OnDecline = onDecline;
            
        }

        public void InvokeAccept()
        {
            DisableAllViews();
            Hide();
            OnAccept?.Invoke();
        }

        public void InvokeDecline()
        {
            DisableAllViews();
            Hide();
            OnDecline?.Invoke();
        }

        #endregion
        #region ChangeRelationshipView

#pragma warning disable 618
        public void DisplayRelationShipView(SyncListInt oldStates, RelationshipState newState)
#pragma warning restore 618
        {
            Show();
            DisableAllViews();
            ChangeRelationShipView.SetActive(true);

            foreach (Transform child in RelationShipListRoot)
            {
                Destroy(child.gameObject);
            }

#pragma warning disable 618
            var alreadySeenRelationships = new HashSet<(NetworkInstanceId, NetworkInstanceId)>();
#pragma warning restore 618
            foreach (var player in GameManager.Instance.Players)
            {
                for (int i = 0; i < player.Diplomacy.PlayerRelationships.Count; i++)
                {
                    var relationship = player.Diplomacy.PlayerRelationships[i];
                    if (oldStates.Contains((int)relationship.State) &&
                        !alreadySeenRelationships.Contains((relationship.PartnerId, player.netId)))
                    {
                        var partner = GameManager.Instance.GetPlayerOfId(relationship.PartnerId);
                        alreadySeenRelationships.Add((player.netId, relationship.PartnerId));
                        string label = $"{relationship}: {player.ToString()} <-> {partner.ToString()}";

                        AddRelationShipRepresantation(label, () =>
                        {
                            player.Diplomacy.CmdSetRelation(relationship.PartnerId, newState);
#pragma warning disable 618
                            ClientScene.FindLocalObject(relationship.PartnerId).GetComponent<Diplomacy>().CmdSetRelation(player.netId, newState);
#pragma warning restore 618
                        });
                        Player.LocalPlayer.MoveEventCardOnGraveyard();
                    }
                }
            }

            Player.LocalPlayer.PopulateRelationshipView = false;
            OnDone = OnClickDoneRelationships;
        }

        private void AddRelationShipRepresantation(string label, SimpleDelegate onClick)
        {
            var controller = Instantiate(RelationShipPrefab).GetComponent<RelationShipRepresantationController>();
            controller.Init(label, onClick);
        }

        public void OnClickDoneRelationships()
        {

            Player.LocalPlayer.State = PlayerState.CurrentPlayer;
        }

        #endregion
    }


    public delegate void SimpleDelegate();
}

