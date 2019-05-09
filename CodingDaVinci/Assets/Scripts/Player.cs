using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public enum PlayerState
    {
        Registration,
        ConquerRegion,
        LeaderCardSelection,
        CurrentPlayer,
        EnemyResourceRemoval,
        BuildingMode,
        ResourceSelection,
        Idle
    }

    /// <summary>
    /// Main class for player objects in the game
    /// </summary>
#pragma warning disable 618
    public sealed class Player : NetworkBehaviour
#pragma warning restore 618
    {
        #region Client Code
#pragma warning disable 618
        [ClientRpc] private void RpcClearPlayersInWarWith()
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "Only run RpcClearPlayersInWarWith on the client");
            PlayersInWarWith.Clear();
        }

#pragma warning disable 618
        [ClientRpc] private void RpcAddPlayerInWarWith(NetworkInstanceId enemyId)
#pragma warning restore 618
        {
            PlayersInWarWith.Add(enemyId);
        }

        /// <summary>
        /// Adds a region to the list of regions owned by the player
        /// </summary>
        /// <param name="regionId"></param>
#pragma warning disable 618
        [ClientRpc] public void RpcAddRegion(NetworkInstanceId regionId)
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "RpcAddRegion is only allowed to get called clientside");
#pragma warning disable 618
            var region = ClientScene.FindLocalObject(regionId).GetComponent<Region>();
#pragma warning restore 618
            if(OwnedRegions.ContainsKey(regionId))
            {
                int fuck = 0;
                fuck++;
            }
            OwnedRegions.Add(regionId, region);
        }

#pragma warning disable 618
        [ClientRpc] public void RpcSetCamera(Vector3 position, Quaternion rotation)
#pragma warning restore 618
        {
            if(isLocalPlayer)
            {
                Camera.main.transform.position = position;
                Camera.main.transform.rotation = rotation;
                Camera.main.transform.Translate(0, 3, 0);
            }
        }

#pragma warning disable 618
        [ClientRpc] private void RpcAddCardToHand(NetworkInstanceId cardId)
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "RpcAddCardToHand is only allowed to get called clientside");
#pragma warning disable 618
            var card = ClientScene.FindLocalObject(cardId).GetComponent<CardDisplay>().Card;
#pragma warning restore 618
            HandCards.Add(card);
        }

#pragma warning disable 618
        [ClientRpc] private void RpcAddLeaderCard(NetworkInstanceId cardId)
        {
            var card = ClientScene.FindLocalObject(cardId).GetComponent<CardDisplay>().Card;
#pragma warning restore 618
            LeaderCards.Add(card);
        }

#pragma warning disable 618
        [ClientRpc] private void RpcRemoveCardFromHand(NetworkInstanceId cardId)
        {
            var card = ClientScene.FindLocalObject(cardId).GetComponent<CardDisplay>().Card;
#pragma warning restore 618
            HandCards.Remove(card);
        }

        private Region SelectedRegion = null;
        #endregion

        #region Shared Code
        private void Awake()
        {
            VictoryPoints = GetComponent<VictoryPoints>();
            Resources = GetComponent<Resources>();
            Diplomacy = GetComponent<Diplomacy>();
        }

        private void OnGUI()
        {
            if(isLocalPlayer)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 200, 20, 200, 200));
                GUILayout.Label($"Round: {GameManager?.Round}");
                GUILayout.EndArea();

                if(IsGameOver)
                {
                    GUILayout.BeginArea(new Rect(200, 200, 200, 200));
                    GUILayout.Label(GameOverWinnerLabel);
                    GUILayout.EndArea();
                }

                if(Diplomacy.ReceivedRequest)
                {
                    Diplomacy.DrawRequestUI();
                }

                switch(State)
                {
                    case PlayerState.EnemyResourceRemoval:
                    {
                        GUILayout.BeginArea(new Rect(400, 200, 200, 200));
                        GUILayout.Label($"Zerstöre 2 Ressourcen von Spieler {PlayersInWarWith[CurrentEnemyToRemoveFrom]}");
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button("Baustoff"))
                        {
                            CmdRemoveResource(PlayersInWarWith[CurrentEnemyToRemoveFrom], ResourceType.ConstructionMaterial);
                        }
                        if(GUILayout.Button("Einfluss"))
                        {
                            CmdRemoveResource(PlayersInWarWith[CurrentEnemyToRemoveFrom], ResourceType.Power);
                        }
                        if(GUILayout.Button("Technologie"))
                        {
                            CmdRemoveResource(PlayersInWarWith[CurrentEnemyToRemoveFrom], ResourceType.Technology);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndArea();
                        break;
                    }

                    case PlayerState.ResourceSelection:
                    {
                        GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 30, 300, 60));
                        GUILayout.Label($"Erhalte {SelectedResourceAmount} Einheiten von");
                        GUILayout.BeginHorizontal();
                        if(GUILayout.Button("Baustoffe"))
                        {
                            Resources.AddConstructionMaterial(SelectedResourceAmount, false);
                            State = PlayerState.CurrentPlayer;
                        }
                        if(GUILayout.Button("Einfluss"))
                        {
                            Resources.AddPower(SelectedResourceAmount, false);
                            State = PlayerState.CurrentPlayer;
                        }
                        if(GUILayout.Button("Technologie"))
                        {
                            Resources.AddTechnology(SelectedResourceAmount, false);
                            State = PlayerState.CurrentPlayer;
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndArea();
                        break;
                    }

                    case PlayerState.BuildingMode:
                    {
                        if(SelectedRegion)
                        {
                            var building = GameManager.GetBuilding(BuildingToBuild);
                            if(SelectedRegion.Owner == this &&
                               SelectedRegion.FulFillsRequirements(building.ConstructionRequirements))
                            {
                                GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, 15, 100, 30));
                                if(GUILayout.Button("Bauen"))
                                {
                                    CmdBuildBuilding(BuildingToBuild, SelectedRegion.netId);
                                    BuildingToBuild = null;
                                    State = PlayerState.CurrentPlayer;
                                }
                                GUILayout.EndArea();
                            }
                        }
                        break;
                    }

                    case PlayerState.CurrentPlayer:
                    {
                        GameManager.StockExchange.Draw(this);
                        GUILayout.BeginArea(new Rect(200, 200, 200, 200));
                        if(GUILayout.Button("Diplomatie"))
                        {
                            ShowDiplomacy = true;
                        }
                        if(GUILayout.Button("Build Building"))
                        {
                            ShowBuildingOverview = true;
                        }
                        if(GUILayout.Button("Buy Technology Card"))
                        {
                            if(Resources.Technology >= 3)
                            {
                                CmdRequestTechnologyCardPurchase();
                            }
                        }
                        if(GUILayout.Button("End Turn"))
                        {
                            CmdEndTurn();
                        }
                        GUILayout.EndArea();

                        if(ShowDiplomacy && !Diplomacy.HasDoneDiplomacyAction)
                        {
                            ShowDiplomacy = Diplomacy.DrawUI();
                        }

                        if(SelectedRegion)
                        {
                            GUILayout.BeginArea(new Rect(300, 20, 100, 100));
                            GUILayout.Label($"Besitzer: {SelectedRegion.Owner?.netId}");
                            bool isNeighbourRegion = false;
                            foreach(Region region in SelectedRegion.NeighbourRegions)
                            {
                                if(OwnedRegions.ContainsValue(region))
                                {
                                    isNeighbourRegion = true;
                                    break;
                                }
                            }
                            if(isNeighbourRegion && SelectedRegion.Owner == null)
                            {
                                if(GUILayout.Button("Conquer"))
                                {
                                    CmdRequestConquerRegion(SelectedRegion.netId, false);
                                }
                            }
                            if((BuildingToBuild != null && BuildingToBuild != "") 
                                && OwnedRegions.ContainsValue(SelectedRegion)
                                && SelectedRegion.Building == null
                                && SelectedRegion.FulFillsRequirements(GameManager.GetBuilding(BuildingToBuild).ConstructionRequirements))
                            {
                                var building = GameManager.GetBuilding(BuildingToBuild);
                                if(GUILayout.Button("Build"))
                                {
                                    if(Resources.HasResources(building.ConstructionCosts))
                                    {
                                        CmdBuildBuilding(BuildingToBuild, SelectedRegion.netId);
                                    }
                                }
                            }
                            GUILayout.EndArea();
                        }

                        if(ShowBuildingOverview)
                        {
                            GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, 100, 400, 500));
                            foreach(var building in GameManager.Buildings)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(building.name);
                                if(GUILayout.Button("Auswählen"))
                                {
                                    BuildingToBuild = building.Type;
                                }
                                GUILayout.EndHorizontal();
                            }
                            if(GUILayout.Button("Zurück"))
                            {
                                ShowBuildingOverview = false;
                            }
                            GUILayout.EndArea();
                        }
                        
                        break;
                    }

                    case PlayerState.ConquerRegion:
                    {
                        if(SelectedRegion)
                        {
                            GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, 20, 100, 30));
                            if(GUILayout.Button("Besetzen"))
                            {
                                CmdRequestConquerRegion(SelectedRegion.netId, true);
                                State = PlayerState.CurrentPlayer;
                            }
                            GUILayout.EndArea();
                        }
                        break;
                    }

                    case PlayerState.Idle:
                    {
                        GUILayout.BeginArea(new Rect(200, 200, 200, 200));
                        GUILayout.Label($"Wait currently ID: {CurrentPlayerNetId.Value} is playing");
                        GUILayout.EndArea();
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            if(isLocalPlayer)
            {
                switch(State)
                {
                    // NOTE: We have to continiously look for the GameManager because for some
                    // reason with the networking it can not be guaranteed to get found in Start
                    // even if it already exists
                    case PlayerState.Registration:
                    {
                        var go = GameObject.Find("GameManager");
                        GameManager = go?.GetComponent<GameManager>();
                        if(GameManager)
                        {
                            CmdRegisterPlayer(GameManager.netId);
                            State = PlayerState.Idle;
                        }
                        break;
                    }

                    case PlayerState.ConquerRegion:
                    {
                        var (success, hit) = MakeHitCheck();
                        if(success)
                        {
                            if(hit.collider.CompareTag("Region"))
                            {
                                var region = hit.collider.GetComponent<Region>();
                                foreach(var requirement in ConquerRequirements.Requirements)
                                {
                                    switch(requirement)
                                    {
                                        case ConquerRequirement.MustBeNoSeaRegion:
                                        {
                                            if(region.HasSeaAccess())
                                            {
                                                SelectedRegion = null;
                                                return;
                                            }
                                            break;
                                        }

                                        case ConquerRequirement.MustBeSeaRegion:
                                        {
                                            if(!region.HasSeaAccess())
                                            {
                                                SelectedRegion = null;
                                                return;
                                            }
                                            break;
                                        }

                                        case ConquerRequirement.MustHaveNoOccupiedNeighbours:
                                        {
                                            foreach(var neighbour in region.NeighbourRegions)
                                            {
                                                if(neighbour.Owner != null)
                                                {
                                                    SelectedRegion = null;
                                                    return;
                                                }
                                            }
                                            break;
                                        }

                                        case ConquerRequirement.MustBeFree:
                                        {
                                            if(region.Owner != null)
                                            {
                                                SelectedRegion = null;
                                                return;
                                            }
                                            break;
                                        }

                                        default:
                                        {
                                            Debug.LogError($"{requirement} is not handled");
                                            break;
                                        }
                                    }
                                }

                                SelectedRegion = region;
                            }
                        }
                        break;
                    }

                    case PlayerState.LeaderCardSelection:
                    {
                        var (success, hit) = MakeHitCheck();
                        if(success && hit.collider.CompareTag("Card"))
                        {
                            var card = hit.collider.GetComponent<CardDisplay>();
                            if(card.Card.Type == CardType.Leader)
                            {
#pragma warning disable 618
                                NetworkInstanceId id = hit.collider.GetComponent<NetworkIdentity>().netId;
#pragma warning restore 618
                                if(HandCards.Contains(hit.collider.GetComponent<CardDisplay>().Card))
                                {
                                    CmdAddLeaderCardFromHand(id);
                                }
                                else
                                {
                                    CmdChooseCard(id);
                                }
                            }
                        }
                        break;
                    }

                    case PlayerState.CurrentPlayer:
                    {
                        var (success, hit) = MakeHitCheck();
                        if(success)
                        {
                            if(hit.collider.CompareTag("Card"))
                            {
                                var card = hit.collider.GetComponent<CardDisplay>().Card;
                                if(HandCards.Contains(card))
                                {
#pragma warning disable 618
                                    CmdPlayCard(hit.collider.GetComponent<NetworkIdentity>().netId);
#pragma warning restore 618
                                }
                            }
                            else if(hit.collider.CompareTag("Region"))
                            {
                                SelectedRegion = hit.collider.GetComponent<Region>();
                            }
                        }
                        break;
                    }

                    case PlayerState.BuildingMode:
                    {
                        var (success, hit) = MakeHitCheck();
                        if(success && hit.collider.CompareTag("Region"))
                        {
                            SelectedRegion = hit.collider.GetComponent<Region>();
                        }
                        break;
                    }

                    case PlayerState.Idle: { break; }
                }
            }
        }

        private (bool, RaycastHit) MakeHitCheck()
        {
            var hit = new RaycastHit();
            if(Input.GetMouseButtonDown(LEFT_MOUSEBUTTON))
            {
                var mouse = new Vector3();
                mouse.x = Input.mousePosition.x;
                mouse.y = Input.mousePosition.y;
                mouse.z = Camera.main.nearClipPlane;
                mouse = Camera.main.ScreenToWorldPoint(mouse);
                Vector3 cameraPosition = Camera.main.transform.position;
                if(Physics.Raycast(cameraPosition, mouse - cameraPosition, out hit))
                {
                    return (true, hit);
                }
            }
            return (false, hit);
        }

        public Vector3 CurrentPlayerMarkPosition()
        {
            Vector3 position = transform.position;
            position += (transform.forward * 4) + (transform.right * 4);
            position.y = 0.225f;
            return position;
        }
        public VictoryPoints VictoryPoints { get; private set; }
        public Resources Resources { get; private set; }
        public Diplomacy Diplomacy {get; private set;}
        public GameManager GameManager {get; private set; }
#pragma warning disable 618
        public List<NetworkInstanceId> PlayersInWarWith { get; private set; } = new List<NetworkInstanceId>();
#pragma warning restore 618

#pragma warning disable 618
        private Dictionary<NetworkInstanceId, Region> OwnedRegions = new Dictionary<NetworkInstanceId, Region>();
#pragma warning restore 618
        private List<Card> LeaderCards = new List<Card>(4);
        private List<Card> HandCards = new List<Card>(4);

#pragma warning disable 618
        [SyncVar] private int RemovedResources = 0;
        [SyncVar] private int CurrentEnemyToRemoveFrom = 0;
        [SyncVar] private bool ShowDiplomacy = false;
        [SyncVar] private bool IsGameOver = false;
        [SyncVar] private string GameOverWinnerLabel;
        [SyncVar] private NetworkInstanceId CurrentPlayerNetId;
        [SyncVar] private int TechnologyConstructionCostReduction = 0;
        [SyncVar] private int EconomyOrSciencePoints = 0;
        [SyncVar] private bool GetEconomyPointsThisRound = true;
        [SyncVar] private int ConstructionMaterialOrTechnologyPoints = 0;
        [SyncVar] private bool GetConstructionPointsThisRound = true;
        [SyncVar] private int OperaPointsBuff = 0;
        [SyncVar, HideInInspector] public float NextRegionCostFactor = 1;
        [SyncVar, HideInInspector] public PlayerState State = PlayerState.Registration;
        [SyncVar, HideInInspector] public ConquerRequirements ConquerRequirements;
        [SyncVar, HideInInspector] public int KindergardenResources = 0;
        [SyncVar] bool HasKindergarden = false;
        [SyncVar, HideInInspector] public int SelectedResourceAmount = 0;
        [SyncVar, HideInInspector] public string BuildingToBuild = null;
        [SyncVar] bool ShowBuildingOverview = false;
#pragma warning restore 618
        #endregion

        #region Server Code
#pragma warning disable 618
        [Command] public void CmdPostTradingOffer(ResourceType offeredResource,
            string amountOffered, ResourceType requestedResource, string amountRequested)
#pragma warning restore 618
        {
            int amountOfferedInt = 0;
            int amountRequestedInt = 0;
            if(int.TryParse(amountOffered, out amountOfferedInt) &&
               int.TryParse(amountRequested, out amountRequestedInt))
            {
                var offer = new TradingOffer
                {
                    Owner = netId,
                    OfferedResource = offeredResource,
                    OfferedAmount = amountOfferedInt,
                    RequestedResource = requestedResource,
                    RequestedAmount = amountRequestedInt
                };

                GameManager.StockExchange.Offers.Add(offer);
            }
        }

#pragma warning disable 618
        [Command] public void CmdRemoveTradingOffer(TradingOffer offer)
#pragma warning restore 618
        {
            GameManager.StockExchange.Offers.Remove(offer);
        }

#pragma warning disable 618

        [Command] public void CmdAcceptTradingOffer(TradingOffer offer)
        {
            int index = Diplomacy.PlayerRelationships.Find(offer.Owner);
            if(index != -1 && 
                (Diplomacy.PlayerRelationships[index].State == RelationshipState.TradingAgreement ||
                 Diplomacy.PlayerRelationships[index].State == RelationshipState.Alliance))
            {
                var ownerResources = NetworkServer.FindLocalObject(offer.Owner).GetComponent<Resources>();
                var ownerResourecesRequired = new ResourceInfo[]
                {
                    new ResourceInfo {Type = offer.OfferedResource, Amount = offer.OfferedAmount}
                };

                var playerResourcesRequired = new ResourceInfo[]
                {
                    new ResourceInfo{Type = offer.RequestedResource, Amount = offer.RequestedAmount}
                };

                if(Resources.HasResources(playerResourcesRequired) &&
                    ownerResources.HasResources(ownerResourecesRequired))
                {
                    var resourcesForOwner = new ResourceInfo[]
                    {
                        new ResourceInfo {Type = offer.OfferedResource, Amount = -offer.OfferedAmount},
                        new ResourceInfo {Type = offer.RequestedResource, Amount = offer.RequestedAmount}
                    };

                    var resourcesForPlayer = new ResourceInfo[]
                    {
                        new ResourceInfo {Type = offer.RequestedResource, Amount = -offer.RequestedAmount},
                        new ResourceInfo {Type = offer.OfferedResource, Amount = offer.OfferedAmount}
                    };

                    ownerResources.AddResources(resourcesForOwner);
                    Resources.AddResources(resourcesForPlayer);
                    GameManager.StockExchange.Offers.Remove(offer);
                }
            }
        }
#pragma warning restore 618

#pragma warning disable 618
        [Command] private void CmdRemoveResource(NetworkInstanceId playerId, ResourceType type)
        {
            Assert.IsTrue(isServer, "Only run CmdRemoveResource on server");
            var resources = NetworkServer.FindLocalObject(playerId).GetComponent<Resources>();
            var resourcesToRemove = new ResourceInfo[]
            {
                new ResourceInfo{Type = type, Amount = -1}
            };
            resources.AddResources(resourcesToRemove);

            RemovedResources++;
            if(RemovedResources == 2)
            {
                CurrentEnemyToRemoveFrom++;
                RemovedResources = 0;
                if(CurrentEnemyToRemoveFrom >= PlayersInWarWith.Count)
                {
                    State = PlayerState.CurrentPlayer;
                    RpcClearPlayersInWarWith();
                }
            }
        }
        #pragma warning restore 618

        #pragma warning disable 618
        public void SelectLeaderCard()
        {
            State = PlayerState.LeaderCardSelection;
            if(HandCards.Find(x => x.Type == CardType.Leader) == null)
            {
                GameManager.RequestLeaderCardsForSelection(netId);
            }
        }

        [Command] private void CmdAddLeaderCardFromHand(NetworkInstanceId cardId)
        {
            AddLeaderCard(cardId);
            RpcRemoveCardFromHand(cardId);
            GameManager.SelectedLeaderCard(netId);
        }

        [Command]
        private void CmdBuildBuilding(string buildingName, NetworkInstanceId regionId)
        {
            foreach(var buildingPrefab in GameManager.Buildings)
            {
                if(buildingPrefab.Type == buildingName)
                {
                    if(buildingName == "Kindergarden")
                    {
                        HasKindergarden = true;
                    }

                    var building = Instantiate(buildingPrefab);
                    var region = NetworkServer.FindLocalObject(regionId).GetComponent<Region>();
                    building.transform.position = region.transform.position;
                    NetworkServer.Spawn(building.gameObject);
                    region.RpcSetBuilding(building.netId);
                    Resources.ApplyConstructionCosts(building.ConstructionCosts, TechnologyConstructionCostReduction);
                    VictoryPoints.ApplyVictoryPoints(building.GainedVictoryPoints);
                    if(buildingName == "Opera")
                    {
                        VictoryPoints.AddCulture(OperaPointsBuff, false);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Request the server to add a region to its own terretory
        /// </summary>
        /// <param name="regionId"></param>
#pragma warning disable 618
        [Command] private void CmdRequestConquerRegion(NetworkInstanceId regionId, bool forFree)
#pragma warning restore 618
        {
#pragma warning disable 618
            var region = NetworkServer.FindLocalObject(regionId).GetComponent<Region>();
#pragma warning restore 618
            if(forFree)
            {
                RpcAddRegion(regionId);
                region.RpcSetOwner(netId);
            }
            uint price = (uint)((region.Owner == null ? 3 : 9) * NextRegionCostFactor);
            if(Resources.Power >= price)
            {
                NextRegionCostFactor = 1;
                Resources.Power -= (int)price;
                RpcAddRegion(regionId);
                region.RpcSetOwner(netId);
            }
        }

#pragma warning disable 618
        [Command] private void CmdRequestTechnologyCardPurchase()
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "CmdRequestTechnologyCardPurchase is only allowed to" +
                " get called serverside");

            if(GameManager.RequestTechnologyCard(netId))
            {
                Resources.Technology -= 3;
            }
        }

#pragma warning disable 618
        public void AddLeaderCard(NetworkInstanceId cardId)
#pragma warning restore 618
        {
            State = PlayerState.Idle;
#pragma warning disable 618
            var card = NetworkServer.FindLocalObject(cardId);
#pragma warning restore 618
            string leaderCardGroup = card.GetComponent<CardDisplay>().Card.className;
            if(LeaderCardsOfGroup.ContainsKey(leaderCardGroup))
            {
                LeaderCardsOfGroup[leaderCardGroup]++;
                // HACK: Ignoring the amount of cards of the group the player has
                // only works because each card raises the buff linear. If this changes
                //at some point of the balancing process this has to be revisited
                switch(leaderCardGroup)
                {
                    case "Förderinnen der Wissenschaft":
                    {
                        
                        TechnologyConstructionCostReduction++;
                        break;
                    }

                    case "Dichterinnen":
                    {
                        VictoryPoints.AddCulture(1, true);
                        break;
                    }

                    case "Vorkämpferinnen der Frauenrechte":
                    {
                        ConstructionMaterialOrTechnologyPoints++;
                        break;
                    }

                    case "Frauen der Musik":
                    {
                        OperaPointsBuff++;
                        break;
                    }

                    case "Herrscherinnen":
                    {
                        Resources.AddPower(1, true);
                        break;
                    }

                    case "Tänzerinnen":
                    {
                        VictoryPoints.AddCulture(1, true);
                        break;
                    }

                    case "Frauen der Politik":
                    {
                        Resources.AddPower(2, true);
                        break;
                    }

                    case "Freiheitskämpferinnen":
                    {
                        Resources.AddPower(1, true);
                        break;
                    }

                    case "Soziale Frauen":
                    {
                        EconomyOrSciencePoints++;
                        break;
                    }
                }
            }
            else
            {
                LeaderCardsOfGroup.Add(leaderCardGroup, 1);
            }

            foreach(var effect in card.GetComponents<CardEffect>())
            {
                if(effect.IsLeaderEffect)
                {
                    effect.Execute(this);
                }
            }

            card.transform.position = transform.position + (transform.right * -2.8f) + (transform.forward * 4) + new Vector3(0, -0.8f, 0);
            RpcAddLeaderCard(cardId);
        }

        public void AddCardToHand(CardDisplay card)
        {
            card.transform.position = transform.position + (transform.forward * 8);
            card.transform.LookAt(transform);
            card.transform.Rotate(0, 180, 0);
            card.transform.position += transform.right * (-1.5f + (HandCards.Count * 2.5f));
#pragma warning disable 618
            RpcAddCardToHand(card.GetComponent<NetworkIdentity>().netId);
            if(card.Card.className == "EventCard")
            {
                CmdPlayCard(card.GetComponent<NetworkIdentity>().netId);
            }
#pragma warning restore 618
        }

#pragma warning disable 618
        [Command] private void CmdPlayCard(NetworkInstanceId cardId)
#pragma warning restore 618
        {
#pragma warning disable 618
            var cardGO = NetworkServer.FindLocalObject(cardId);
#pragma warning restore 618
            var card = cardGO.GetComponent<CardDisplay>().Card;
            var effects = new List<CardEffect>();
            if(card.Type == CardType.Technology)
            {
                var technologyCardStack = 
                    GameObject.Find("TechnologyCards").GetComponent<TechnologyCardStack>();

                effects.AddRange(cardGO.GetComponents<CardEffect>());

                cardGO.transform.position = technologyCardStack.Graveyard.position;
                cardGO.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            else if(card.Type == CardType.Leader)
            {
                var leaderCardStack =
                    GameObject.Find("LeaderCards").GetComponent<MainCardStack>();

                cardGO.transform.position = leaderCardStack.Graveyard.position;
                cardGO.transform.rotation = Quaternion.Euler(90, 0, 0);

                foreach(var effect in cardGO.GetComponents<CardEffect>())
                {
                    if(!effect.IsLeaderEffect)
                    {
                        effects.Add(effect);
                    }
                }
            }
            else if(card.Type == CardType.OldMaid)
            {
                var leaderCardStack =
                    GameObject.Find("LeaderCards").GetComponent<MainCardStack>();

                cardGO.transform.position = leaderCardStack.Graveyard.position;
                cardGO.transform.rotation = Quaternion.Euler(90, 0, 0);

                effects.AddRange(cardGO.GetComponents<CardEffect>());
            }

            foreach(var effect in effects)
            {
                effect.Execute(this);
            }

            RpcRemoveCardFromHand(cardId);

        }

        /// <summary>
        /// Requests the server to register the player at the <see cref="GameManager"/>
        /// </summary>
#pragma warning disable 618
        [Command] private void CmdRegisterPlayer(NetworkInstanceId gameManagerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only execute CmdRegisterPlayerOnServer");
#pragma warning disable 618
            GameManager = NetworkServer.FindLocalObject(gameManagerId).GetComponent<GameManager>();
#pragma warning restore 618
            GameManager.RegisterPlayer(this);
            State = PlayerState.Idle;
        }

        /// <summary>
        /// Requests the server to end the turn of the player
        /// </summary>
#pragma warning disable 618
        [Command] private void CmdEndTurn()
#pragma warning restore 618
        {
            if(isServer)
            {
                State = PlayerState.Idle;
                GameManager.EndPlayerTurn();
            }
        }

#pragma warning disable 618
        [Command] private void CmdChooseCard(NetworkInstanceId netId)
#pragma warning restore 618
        {
            GameManager.RequestLeaderCard(this.netId, netId);
        }

        /// <summary>
        /// Makes the player the current player to take its turn
        /// </summary>
        public void MakeCurrentPlayer()
        {
            if(isServer)
            {
                State = PlayerState.CurrentPlayer;
                Diplomacy.HasDoneDiplomacyAction = false;
                GameManager.RequestMainCardStackTopCard(netId);
                Resources.ApplyPerRoundValues();

                var playersInWarWith = Diplomacy.PlayerRelationships.FindAll(RelationshipState.War);
                if(playersInWarWith.Count > 0)
                {
                    State = PlayerState.EnemyResourceRemoval;
                    CurrentEnemyToRemoveFrom = 0;
                    foreach(var enemy in playersInWarWith)
                    {
                        RpcAddPlayerInWarWith(enemy.PartnerId);
                    }
                }

                for(int i = 0; i < Diplomacy.PlayerRelationships.FindAll(RelationshipState.TradingAgreement).Count; i++)
                {
                    ResourceType resource = (ResourceType)Random.Range(0, (int)ResourceType.Count);
                    switch(resource)
                    {
                        case ResourceType.ConstructionMaterial: { Resources.ConstructionMaterial++; break; }
                        case ResourceType.Power: { Resources.Power++; break; }
                        case ResourceType.Technology: { Resources.Technology++; break; }
                    }
                }

                for(int i = 0; i < Diplomacy.PlayerRelationships.FindAll(RelationshipState.Alliance).Count * 2; i++)
                {
                    ResourceType resource = (ResourceType)Random.Range(0, (int)ResourceType.Count);
                    switch(resource)
                    {
                        case ResourceType.ConstructionMaterial:
                        { Resources.ConstructionMaterial++; break; }
                        case ResourceType.Power:
                        { Resources.Power++; break; }
                        case ResourceType.Technology:
                        { Resources.Technology++; break; }
                    }
                }

                if(HasKindergarden)
                {
                    for(int i = 0; i < KindergardenResources; i++)
                    {
                        ResourceType resource = (ResourceType)Random.Range(0, (int)ResourceType.Count);
                        switch(resource)
                        {
                            case ResourceType.ConstructionMaterial:
                            { Resources.ConstructionMaterial++; break; }
                            case ResourceType.Power:
                            { Resources.Power++; break; }
                            case ResourceType.Technology:
                            { Resources.Technology++; break; }
                        }
                    }
                }

                if(GetEconomyPointsThisRound)
                {
                    VictoryPoints.AddEconomy(EconomyOrSciencePoints, false);
                }
                else
                {
                    VictoryPoints.AddScience(EconomyOrSciencePoints, false);
                }
                GetEconomyPointsThisRound = !GetEconomyPointsThisRound;

                if(GetConstructionPointsThisRound)
                {
                    Resources.ConstructionMaterial += ConstructionMaterialOrTechnologyPoints;
                }
                else
                {
                    Resources.Technology += ConstructionMaterialOrTechnologyPoints;
                }
                GetConstructionPointsThisRound = !GetConstructionPointsThisRound;

                foreach(var region in OwnedRegions.Values)
                {
                    if(region.Building)
                    {
                        Resources.AddResources(region.Building.GeneratedResources);
                        if(region.Building.Type == "Opera")
                        {
                            VictoryPoints.AddCulture(OperaPointsBuff, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Informs the player about which player takes currently its turn
        /// </summary>
        /// <param name="netId">netId of the player curretnly in control</param>
#pragma warning disable 618
        public void SetCurrentPlayerId(NetworkInstanceId netId)
#pragma warning restore 618
        {
            CurrentPlayerNetId = netId;
        }

#pragma warning disable 618
        public void SendWinner(NetworkInstanceId winner)
#pragma warning restore 618
        {
            if(isServer)
            {
                IsGameOver = true;
                GameOverWinnerLabel = $"Game is over ID: {winner.Value} won!";
            }
        }

#pragma warning disable 618
        public void SendTie(NetworkInstanceId[] winners)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer);
            IsGameOver = true;
            GameOverWinnerLabel = $"Game ended tied. Winners are ID: {string.Join(", ID: ", winners)}";
        }

        private Dictionary<string, int> LeaderCardsOfGroup = new Dictionary<string, int>();
        #endregion
        const int LEFT_MOUSEBUTTON = 0;
    }
}
