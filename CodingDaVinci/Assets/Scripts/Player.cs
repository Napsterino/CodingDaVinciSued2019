using System.Collections.Generic;
using System.Linq;
using System.Collections;
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
        ChangeRelationship,
        Idle
    }

    /// <summary>
    /// Main class for player objects in the game
    /// </summary>
#pragma warning disable 618
    public sealed class Player : NetworkBehaviour
#pragma warning restore 618
    {
        private static Player localPlayer;
        public static Player LocalPlayer
        {
            get
            {
                if (localPlayer == null)
                {
                    GameManager GM = GameManager.Instance;
                    if (GM == null)
                    {
                        GM = FindObjectOfType<GameManager>();
                    }
                    if ((GM != null) && GM.Players != null)
                    {
                        foreach (Player player in GameManager.Instance.Players)
                        {
                            if (player.isLocalPlayer)
                            {
                                localPlayer = player;
                            }
                        }
                    }
                }
                return localPlayer;
            }
        }

        #region Client Code
#pragma warning disable 618
        [ClientRpc]
        public void RpcSetOldRelationships(RelationshipState[] oldRelationships)
#pragma warning restore 618
        {
            OldRelationshipStates.Clear();
            foreach (var element in OldRelationshipStates)
            {
                OldRelationshipStates.Add(element);
            }
        }

#pragma warning disable 618
        [ClientRpc]
        private void RpcClearPlayersInWarWith()
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "Only run RpcClearPlayersInWarWith on the client");
            PlayersInWarWith.Clear();
        }

#pragma warning disable 618
        [ClientRpc]
        private void RpcAddPlayerInWarWith(NetworkInstanceId enemyId)
#pragma warning restore 618
        {
            PlayersInWarWith.Add(enemyId);
        }

        /// <summary>
        /// Adds a region to the list of regions owned by the player
        /// </summary>
        /// <param name="regionId"></param>
#pragma warning disable 618
        [ClientRpc]
        public void RpcAddRegion(NetworkInstanceId regionId)
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "RpcAddRegion is only allowed to get called clientside");
#pragma warning disable 618
            var region = ClientScene.FindLocalObject(regionId).GetComponent<Region>();
#pragma warning restore 618
            if (OwnedRegions.ContainsKey(regionId))
            {
                int fuck = 0;
                fuck++;
            }
            OwnedRegions.Add(regionId, region);
        }

#pragma warning disable 618
        [ClientRpc]
        public void RpcSetCamera(Vector3 position, Quaternion rotation)
#pragma warning restore 618
        {
            if (isLocalPlayer)
            {
                Camera.main.transform.position = position;
                Camera.main.transform.rotation = rotation;
                Camera.main.transform.Translate(0, 3, 0);
            }
        }

#pragma warning disable 618
        [ClientRpc]
        private void RpcAddLeaderCard(NetworkInstanceId cardId)
        {
            var card = ClientScene.FindLocalObject(cardId).GetComponent<CardDisplay>().Card;
#pragma warning restore 618
            LeaderCards.Add(card);
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

#pragma warning disable 618
        [SyncVar]
#pragma warning restore 618
        private bool processGUINextFrame = false;
        public void ProcessGUINextFrame()
        {
            if (isLocalPlayer)
            {
                processGUINextFrame = true;
            }
            else
            {
                LocalPlayer.ProcessGUINextFrame();
            }
        }

#pragma warning disable 618
        [ClientRpc]
#pragma warning restore 618
        public void RpcProcessAllGUIs()
        {
            foreach (Player player in GameManager.Players)
            {
                player.ProcessGUINextFrame();
            }
        }

#if NEW_GUI

        private void ProcessGUI()
        {
            processGUINextFrame = false;

            if (isLocalPlayer)
            {
                MainUIRoot.Instance.CheckHideAll();
                MainUIRoot.Instance.VictoryPointsPanel.Science.SetAmount(VictoryPoints.Science);
                MainUIRoot.Instance.VictoryPointsPanel.Territory.SetAmount(VictoryPoints.Territory);
                MainUIRoot.Instance.VictoryPointsPanel.Economy.SetAmount(VictoryPoints.Economy);
                MainUIRoot.Instance.VictoryPointsPanel.Culture.SetAmount(VictoryPoints.Culture);
                MainUIRoot.Instance.VictoryPointsPanel.TotalVictoryPoints.SetAmount(VictoryPoints.WinPoints);
                MainUIRoot.Instance.ResourcesPanel.Construction.SetAmount(Resources.ConstructionMaterial);
                MainUIRoot.Instance.ResourcesPanel.Technology.SetAmount(Resources.Technology);
                MainUIRoot.Instance.ResourcesPanel.Influence.SetAmount(Resources.Power);

                MainUIRoot.Instance.CurrentRound.SetAmount(GameManager.Round);

                if (IsGameOver && !MainUIRoot.Instance.GameOverPanel.Panel.activeSelf)
                {
                    MainUIRoot.Instance.SetGameOver(GameOverWinnerLabel);
                }

                if (Diplomacy.UpdateGUIForRequest)
                {
                    Diplomacy.UpdateGUIForRequest = false;
                    ShowDiplomacy = true;
                    Diplomacy.SetupRequestUI();
                }

                switch (State)
                {
                    case PlayerState.EnemyResourceRemoval:
                        {
                            List<Player> playersInWar = new List<Player>();
                            foreach (var id in PlayersInWarWith)
                            {
#pragma warning disable 618
                                playersInWar.Add(ClientScene.FindLocalObject(id).GetComponent<Player>());
#pragma warning restore 618
                            }
                            MainUIRoot.Instance.SetupUITurnBegin(true, playersInWar);
                            break;
                        }

                    case PlayerState.ResourceSelection:
                        {
                            MainUIRoot.Instance.Trading.ShowResourcesSelectorView($"Erhalte {SelectedResourceAmount} Resourcen:", SelectedResourceAmount);
                            break;
                        }

                    case PlayerState.BuildingMode:
                        {
                            if (SelectedRegion)
                            {
                                var building = GameManager.GetBuilding(BuildingsToBuild[0]);
                                if (SelectedRegion.Owner == this &&
                                    SelectedRegion.FulFillsRequirements(building.ConstructionRequirements))
                                {
                                    MainUIRoot.Instance.ContextMenu.UpdateContextMenu($"Bauen", Translationhelper.Get(building.name), () =>
                                                                                      {
                                                                                          CmdBuildBuilding(BuildingsToBuild[0], SelectedRegion.netId, true);
                                                                                          CmdRemoveBuildingFromBuildList();
                                                                                          ProcessGUINextFrame();
                                                                                      });
                                }
                            }
                            break;
                        }

                    case PlayerState.CurrentPlayer:
                        {
                            MainUIRoot.Instance.MainPanel.Show();

                            if (SelectedRegion)
                            {
                                string label;
                                if (SelectedRegion.Owner)
                                {
                                    label = $"Besitzer: {SelectedRegion.Owner}";
                                }
                                else
                                {
                                    label = "Kein Besitzer";
                                }
                                bool isNeighbourRegion = false;
                                foreach (Region region in SelectedRegion.NeighbourRegions)
                                {
                                    if (OwnedRegions.ContainsValue(region))
                                    {
                                        isNeighbourRegion = true;
                                        break;
                                    }
                                }
                                if (isNeighbourRegion)
                                {
                                    if (SelectedRegion.Owner == null)
                                    {
                                        MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Annektieren", label, () =>
                                                                                          {
                                                                                              CmdRequestConquerRegion(SelectedRegion.netId, false);
                                                                                              ProcessGUINextFrame();
                                                                                          });
                                    }
                                    else
                                    {
                                        var building = GameManager.GetBuilding(BuildingToBuild);
                                        if (building != null)
                                        {
                                            label += $"\n{Translationhelper.Get(building.name)}";
                                            MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Bauen", label, () =>
                                                                                              {
                                                                                                  if (Resources.HasResources(building.ConstructionCosts))
                                                                                                  {
                                                                                                      CmdBuildBuilding(BuildingToBuild, SelectedRegion.netId, false);
                                                                                                  }
                                                                                                  ProcessGUINextFrame();
                                                                                              });
                                        }
                                    }
                                }
                                if ((BuildingToBuild != null && BuildingToBuild != "")
                                    && OwnedRegions.ContainsValue(SelectedRegion)
                                    && SelectedRegion.Building == null
                                    && SelectedRegion.FulFillsRequirements(GameManager.GetBuilding(BuildingToBuild).ConstructionRequirements))
                                {
                                    var building = GameManager.GetBuilding(BuildingToBuild);
                                    label += $"\n{building.name}";
                                    MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Bauen", label, () =>
                                                                                      {
                                                                                          if (Resources.HasResources(building.ConstructionCosts))
                                                                                          {
                                                                                              CmdBuildBuilding(BuildingToBuild, SelectedRegion.netId, false);
                                                                                          }
                                                                                          ProcessGUINextFrame();
                                                                                      });
                                }
                            }

                            if (ShowBuildingOverview)
                            {
                                MainUIRoot.Instance.BuildingMenu.Show();
                            }

                            break;
                        }

                    case PlayerState.ChangeRelationship:
                        {
                            if (PopulateRelationshipView)
                            {
                                MainUIRoot.Instance.Diplomacy.DisplayRelationShipView(OldRelationshipStates, NewRelationshipState);
                            }
                            else
                            {
                                MainUIRoot.Instance.Diplomacy.Show();
                            }
                            break;
                        }

                    case PlayerState.ConquerRegion:
                        {
                            if (SelectedRegion)
                            {
                                if (RegionGetsAddedToPlayer)
                                {
                                    MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Besetzen", "", () =>
                                                                                      {
                                                                                          CmdRequestConquerRegion(SelectedRegion.netId, true);
                                                                                          State = PlayerState.CurrentPlayer;
                                                                                          ProcessGUINextFrame();
                                                                                      });
                                }
                                else
                                {
                                    MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Befreien", "", () =>
                                                                                      {
                                                                                          CmdFreeRegion(SelectedRegion.netId, true);
                                                                                          State = PlayerState.CurrentPlayer;
                                                                                          ProcessGUINextFrame();
                                                                                      });
                                }
                            }
                            break;
                        }

                    case PlayerState.Idle:
                        {
                            Player currentPlayer = null;
                            foreach (Player player in GameManager.Players)
                            {
                                if (player.netId == CurrentPlayerNetId)
                                {
                                    currentPlayer = player;
                                }
                            }
                            if (currentPlayer)
                            {
                                MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Bitte Warten", $"{currentPlayer} is playing", null);
                            }
                            else
                            {
                                MainUIRoot.Instance.ContextMenu.UpdateContextMenu("Bitte Warten", "", null);
                            }
                            MainUIRoot.Instance.ContextMenu.ContextButton.interactable = false;
                            break;
                        }
                }
            }
            else
            {
                LocalPlayer.ProcessGUI();
            }
        }

#else
        private void OnGUI()
        {
            if (isLocalPlayer)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 200, 20, 200, 200));
                GUILayout.Label($"Round: {GameManager?.Round}");
                GUILayout.EndArea();
                
                if (IsGameOver)
                {
                    GUILayout.BeginArea(new Rect(200, 200, 200, 200));
                    GUILayout.Label(GameOverWinnerLabel);
                    GUILayout.EndArea();
                }
                
                if (Diplomacy.ReceivedRequest)
                {
                    Diplomacy.DrawRequestUI();
                }
                
                switch (State)
                {
                    case PlayerState.EnemyResourceRemoval:
                    {
                        GUILayout.BeginArea(new Rect(400, 200, 200, 200));
                        GUILayout.Label($"Zerstöre 2 Ressourcen von Spieler {PlayersInWarWith[CurrentEnemyToRemoveFrom]}");
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Baustoff"))
                        {
                            CmdRemoveResource(PlayersInWarWith[CurrentEnemyToRemoveFrom], ResourceType.ConstructionMaterial);
                        }
                        if (GUILayout.Button("Einfluss"))
                        {
                            CmdRemoveResource(PlayersInWarWith[CurrentEnemyToRemoveFrom], ResourceType.Power);
                        }
                        if (GUILayout.Button("Technologie"))
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
                        if (GUILayout.Button("Baustoffe"))
                        {
                            Resources.AddConstructionMaterial(SelectedResourceAmount, false);
                            State = PlayerState.CurrentPlayer;
                        }
                        if (GUILayout.Button("Einfluss"))
                        {
                            Resources.AddPower(SelectedResourceAmount, false);
                            State = PlayerState.CurrentPlayer;
                        }
                        if (GUILayout.Button("Technologie"))
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
                        if (SelectedRegion)
                        {
                            var building = GameManager.GetBuilding(BuildingsToBuild[0]);
                            if (SelectedRegion.Owner == this &&
                                SelectedRegion.FulFillsRequirements(building.ConstructionRequirements))
                            {
                                GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, 15, 100, 30));
                                if (GUILayout.Button("Bauen"))
                                {
                                    CmdBuildBuilding(BuildingsToBuild[0], SelectedRegion.netId, true);
                                    CmdRemoveBuildingFromBuildList();
                                }
                                GUILayout.EndArea();
                            }
                        }
                        break;
                    }
                    
                    case PlayerState.CurrentPlayer:
                    {
                        GameManager.StockExchange.Draw(this);
                        GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 200, 200, 200));
                        if (GUILayout.Button("Diplomatie"))
                        {
                            ShowDiplomacy = true;
                        }
                        if (GUILayout.Button("Build Building"))
                        {
                            ShowBuildingOverview = true;
                        }
                        if (GUILayout.Button("Buy Technology Card"))
                        {
                            if (Resources.Technology >= 3)
                            {
                                CmdRequestTechnologyCardPurchase();
                            }
                        }
                        if (GUILayout.Button("End Turn"))
                        {
                            CmdEndTurn();
                        }
                        GUILayout.EndArea();
                        
                        if (ShowDiplomacy && !Diplomacy.HasDoneDiplomacyAction)
                        {
                            ShowDiplomacy = Diplomacy.DrawUI();
                        }
                        
                        if (SelectedRegion)
                        {
                            GUILayout.BeginArea(new Rect(300, 20, 100, 100));
                            GUILayout.Label($"Besitzer: {SelectedRegion.Owner?.netId}");
                            bool isNeighbourRegion = false;
                            foreach (Region region in SelectedRegion.NeighbourRegions)
                            {
                                if (OwnedRegions.ContainsValue(region))
                                {
                                    isNeighbourRegion = true;
                                    break;
                                }
                            }
                            if (isNeighbourRegion)
                            {
                                if(SelectedRegion.Owner == null)
                                {
                                    if (GUILayout.Button("Conquer"))
                                    {
                                        CmdRequestConquerRegion(SelectedRegion.netId, false);
                                    }
                                }
                                else
                                {
                                    if(GUILayout.Button("Free"))
                                    {
                                        CmdFreeRegion(SelectedRegion.netId, false);
                                    }
                                }
                                
                            }
                            if ((BuildingToBuild != null && BuildingToBuild != "")
                                && OwnedRegions.ContainsValue(SelectedRegion)
                                && SelectedRegion.Building == null
                                && SelectedRegion.FulFillsRequirements(GameManager.GetBuilding(BuildingToBuild).ConstructionRequirements))
                            {
                                var building = GameManager.GetBuilding(BuildingToBuild);
                                if (GUILayout.Button("Build"))
                                {
                                    if (Resources.HasResources(building.ConstructionCosts))
                                    {
                                        CmdBuildBuilding(BuildingToBuild, SelectedRegion.netId, false);
                                    }
                                }
                            }
                            GUILayout.EndArea();
                        }
                        
                        if (ShowBuildingOverview)
                        {
                            GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, 100, 400, 500));
                            foreach (var building in GameManager.Buildings)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(building.name);
                                if (GUILayout.Button("Auswählen"))
                                {
                                    BuildingToBuild = building.Type;
                                }
                                GUILayout.EndHorizontal();
                            }
                            if (GUILayout.Button("Zurück"))
                            {
                                ShowBuildingOverview = false;
                            }
                            GUILayout.EndArea();
                        }
                        
                        break;
                    }
                    
                    case PlayerState.ChangeRelationship:
                    {
#pragma warning disable 618
                        var alreadySeenRelationships = new HashSet<(NetworkInstanceId, NetworkInstanceId)>();
#pragma warning restore 618
                        GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 200, 200, 400));
                        foreach (var player in GameManager.Players)
                        {
                            for (int i = 0; i < player.Diplomacy.PlayerRelationships.Count; i++)
                            {
                                var relationship = player.Diplomacy.PlayerRelationships[i];
                                if (OldRelationshipStates.Contains((int)relationship.State) &&
                                    !alreadySeenRelationships.Contains((relationship.PartnerId, player.netId)))
                                {
                                    alreadySeenRelationships.Add((player.netId, relationship.PartnerId));
                                    if (GUILayout.Button($"ID:{player.netId} and ID:{relationship.PartnerId}"))
                                    {
                                        player.Diplomacy.CmdSetRelation(relationship.PartnerId, NewRelationshipState);
#pragma warning disable 618
                                        ClientScene.FindLocalObject(relationship.PartnerId).GetComponent<Diplomacy>().CmdSetRelation(player.netId, NewRelationshipState);
#pragma warning restore 618
                                    }
                                    MoveEventCardOnGraveyard();
                                }
                            }
                        }
                        GUILayout.EndArea();
                        break;
                    }
                    
                    case PlayerState.ConquerRegion:
                    {
                        if (SelectedRegion)
                        {
                            GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, 20, 100, 30));
                            if (RegionGetsAddedToPlayer)
                            {
                                CmdRequestConquerRegion(SelectedRegion.netId, true);
                                State = PlayerState.CurrentPlayer;
                                MoveEventCardOnGraveyard();
                            }
                            else
                            {
                                CmdFreeRegion(SelectedRegion.netId, true);
                                State = PlayerState.CurrentPlayer;
                                MoveEventCardOnGraveyard();
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
#endif

        private void Update()
        {
            if (isLocalPlayer)
            {
#if NEW_GUI
                if (State > PlayerState.Registration)
                {
                    ProcessGUI();
                }
                //if (processGUINextFrame) ProcessGUI();
#endif
                if (Input.GetKeyDown(KeyCode.End))
                {
                    CmdEndTurn();
                }
                if (Input.GetKeyDown(KeyCode.Home))
                {
                    RpcProcessAllGUIs();
                }
                {
                    var (success, hit) = MakeHitCheck(RIGHT_MOUSEBUTTON);
                    if(success)
                    {
                        if(hit.collider.CompareTag("Card"))
                        {
                            var card = hit.collider.GetComponent<CardDisplay>();
                            Debug.Log($"{HandCards.Count}");

                            if(HandCards.Contains(card.netId))
                            {
                                if(card.IsHoverd)
                                {
                                    CmdUnHoverCard(card.netId);
                                }
                                else
                                {
                                    CmdHoverCard(card.netId);
                                }
                            }
                        }
                    }

                }

                switch (State)
                {
                    // NOTE: We have to continiously look for the GameManager because for some
                    // reason with the networking it can not be guaranteed to get found in Start
                    // even if it already exists
                    case PlayerState.Registration:
                        {
                            //HoverdCardAnchor = transform.GetChild(0);
                            var go = GameObject.Find("GameManager");
                            GameManager = go?.GetComponent<GameManager>();
                            if (GameManager)
                            {
                                string name = PersistentSettingsManager.playerName;
                                Color color = PersistentSettingsManager.playerColor;
                                CmdSetPlayerData(name, color);
                                CmdRegisterPlayer(GameManager.netId);
                                State = PlayerState.Idle;
                            }
                            break;
                        }

                    case PlayerState.ConquerRegion:
                        {
                            var (success, hit) = MakeHitCheck(LEFT_MOUSEBUTTON);
                            if (success)
                            {
                                if (hit.collider.CompareTag("Region"))
                                {
                                    var region = hit.collider.GetComponent<Region>();
                                    foreach (var requirement in ConquerRequirements.Requirements)
                                    {
                                        switch (requirement)
                                        {
                                            case ConquerRequirement.MustBeNoSeaRegion:
                                                {
                                                    if (region.HasSeaAccess())
                                                    {
                                                        SelectedRegion = null;
                                                        return;
                                                    }
                                                    break;
                                                }

                                            case ConquerRequirement.MustBeSeaRegion:
                                                {
                                                    if (!region.HasSeaAccess())
                                                    {
                                                        SelectedRegion = null;
                                                        return;
                                                    }
                                                    break;
                                                }

                                            case ConquerRequirement.MustHaveNoOccupiedNeighbours:
                                                {
                                                    foreach (var neighbour in region.NeighbourRegions)
                                                    {
                                                        if (neighbour.Owner != null)
                                                        {
                                                            SelectedRegion = null;
                                                            return;
                                                        }
                                                    }
                                                    break;
                                                }

                                            case ConquerRequirement.MustBeFree:
                                                {
                                                    if (region.Owner != null)
                                                    {
                                                        SelectedRegion = null;
                                                        return;
                                                    }
                                                    break;
                                                }

                                            case ConquerRequirement.MustBeOccupiedByOtherPlayer:
                                                {
                                                    if (region.Owner == null || region.Owner == this || region.IsStartRegion())
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
                            var (success, hit) = MakeHitCheck(LEFT_MOUSEBUTTON);
                            if (success && hit.collider.CompareTag("Card"))
                            {
                                var card = hit.collider.GetComponent<CardDisplay>();
                                if (card.Card.Type == CardType.Leader)
                                {
                                    Debug.Log("Hit Leader Card");
#pragma warning disable 618
                                    NetworkInstanceId id = hit.collider.GetComponent<NetworkIdentity>().netId;
#pragma warning restore 618
                                    if (HandCards.Contains(hit.collider.GetComponent<CardDisplay>().netId))
                                    {
                                        Debug.Log("Get Leader Card From Hand");
                                        CmdAddLeaderCardFromHand(id);
                                    }
                                    else
                                    {
                                        Debug.Log("Get Leader Card from Stack");
                                        CmdChooseCard(id);
                                    }
                                    ProcessGUINextFrame();
                                }
                            }
                            break;
                        }

                    case PlayerState.CurrentPlayer:
                        {
                            var (success, hit) = MakeHitCheck(LEFT_MOUSEBUTTON);
                            if (success)
                            {
                                if (hit.collider.CompareTag("Card"))
                                {
                                    var card = hit.collider.GetComponent<CardDisplay>();
                                    if (HandCards.Contains(card.netId))
                                    {
#pragma warning disable 618
                                        CmdPlayCard(hit.collider.GetComponent<NetworkIdentity>().netId);
#pragma warning restore 618
                                    }
                                }
                                else if (hit.collider.CompareTag("Region"))
                                {
                                    SelectedRegion = hit.collider.GetComponent<Region>();
                                }
                            }
                        break;
                        }

                    case PlayerState.BuildingMode:
                        {
                            var (success, hit) = MakeHitCheck(LEFT_MOUSEBUTTON);
                            if (success && hit.collider.CompareTag("Region"))
                            {
                                SelectedRegion = hit.collider.GetComponent<Region>();
                            }
                            break;
                        }

                    case PlayerState.Idle:
                        { break; }
                }

            }
        }

        private (bool, RaycastHit) MakeHitCheck(int mouseButton)
        {
            var hit = new RaycastHit();
            if (Input.GetMouseButtonDown(mouseButton))
            {
                var mouse = new Vector3();
                mouse.x = Input.mousePosition.x;
                mouse.y = Input.mousePosition.y;
                mouse.z = Camera.main.nearClipPlane;
                mouse = Camera.main.ScreenToWorldPoint(mouse);
                Vector3 cameraPosition = Camera.main.transform.position;
                if (Physics.Raycast(cameraPosition, mouse - cameraPosition, out hit))
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
        public Diplomacy Diplomacy { get; private set; }
        public GameManager GameManager { get; private set; }
#pragma warning disable 618
        public List<NetworkInstanceId> PlayersInWarWith { get; private set; } = new List<NetworkInstanceId>();
#pragma warning restore 618

#pragma warning disable 618
        public Dictionary<NetworkInstanceId, Region> OwnedRegions = new Dictionary<NetworkInstanceId, Region>();
#pragma warning restore 618
        private List<Card> LeaderCards = new List<Card>(4);

#pragma warning disable 618
        [SyncVar] private int RemovedResources = 0;
        [SyncVar] private int CurrentEnemyToRemoveFrom = 0;
        [SyncVar] public bool ShowDiplomacy = false;
        [SyncVar] public bool PopulateRelationshipView = false;
        [SyncVar] public bool ShowTrading = false;
        [SyncVar] private bool IsGameOver = false;
        [SyncVar] private string GameOverWinnerLabel;
        [SyncVar] private NetworkInstanceId CurrentPlayerNetId;
        [SyncVar] private int TechnologyConstructionCostReduction = 0;
        [SyncVar] private int EconomyOrSciencePoints = 0;
        [SyncVar] private bool GetEconomyPointsThisRound = true;
        [SyncVar] private int OperaPointsBuff = 0;
        [SyncVar, HideInInspector] public float NextRegionCostFactor = 1;
        [SyncVar, HideInInspector] public PlayerState State = PlayerState.Registration;
        [SyncVar, HideInInspector] public ConquerRequirements ConquerRequirements;
        [SyncVar, HideInInspector] public int KindergardenResources = 0;
        [SyncVar] bool HasKindergarden = false;
        [SyncVar, HideInInspector] public int SelectedResourceAmount = 0;
        [HideInInspector] public SyncListString BuildingsToBuild = new SyncListString();
        [SyncVar] public string BuildingToBuild = null;
        [SyncVar] public bool ShowBuildingOverview = false;
        [SyncVar, HideInInspector] public int RandomResourcesOnTurnStart = 0;
        [HideInInspector] public SyncListInt OldRelationshipStates = new SyncListInt();
        [SyncVar, HideInInspector] public RelationshipState NewRelationshipState;
        [SyncVar, HideInInspector] public bool RegionGetsAddedToPlayer;
        [SyncVar] public string PlayerName;
        [SyncVar] public Color PlayerColor;
        [SyncVar] NetworkInstanceId ShownEventCard = NetworkInstanceId.Invalid;
        [SerializeField] Transform AnchorSlot;
        [SerializeField] Transform EventCardSlot;
        [SerializeField] Transform LeaderCardAnchor;
        NetworkIdList HandCards = new NetworkIdList();
#pragma warning restore 618
        #endregion

        #region Server Code
#pragma warning disable 618
        [Command]
        private void CmdFreeRegion(NetworkInstanceId regionId, bool forFree)
#pragma warning restore 618
        {
            if (!forFree)
            {
                if (Resources.Power >= 9)
                {
                    Resources.Power -= 9;
                }
                else
                {
                    return;
                }
            }
#pragma warning disable 618
            var region = NetworkServer.FindLocalObject(regionId).GetComponent<Region>();
            region.RpcSetOwner(NetworkInstanceId.Invalid);
#pragma warning restore 618
        }

#pragma warning disable 618
        [Command]
        private void CmdHoverCard(NetworkInstanceId cardId)
        {
            Vector3 position = AnchorSlot.position;
            NetworkServer.FindLocalObject(cardId).GetComponent<CardDisplay>().Hover(position);
        }
#pragma warning restore 618

#pragma warning disable 618
        [Command]
        private void CmdUnHoverCard(NetworkInstanceId cardId)
        {
            NetworkServer.FindLocalObject(cardId).GetComponent<CardDisplay>().UnHover();
        }
#pragma warning restore 618

#pragma warning disable 618
        [Command]
        public void CmdPostTradingOffer(ResourceType offeredResource,
                                            string amountOffered, ResourceType requestedResource, string amountRequested)
#pragma warning restore 618
        {
            int amountOfferedInt = 0;
            int amountRequestedInt = 0;
            if (int.TryParse(amountOffered, out amountOfferedInt) &&
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
        [Command]
        public void CmdRemoveTradingOffer(TradingOffer offer)
#pragma warning restore 618
        {
            GameManager.StockExchange.Offers.Remove(offer);
        }

#pragma warning disable 618
        [Command]
        private void CmdRemoveBuildingFromBuildList()
#pragma warning restore 618
        {
            BuildingsToBuild.RemoveAt(0);
            if (BuildingsToBuild.Count == 0)
            {
                State = PlayerState.CurrentPlayer;
                MoveEventCardOnGraveyard();
            }
        }

        public void MoveEventCardOnGraveyard()
        {
#pragma warning disable 618
            if (ShownEventCard != NetworkInstanceId.Invalid)
#pragma warning restore 618
            {
                var leaderCardStack =
                    GameObject.Find("LeaderCards").GetComponent<MainCardStack>();
#pragma warning disable 618
                var card = NetworkServer.FindLocalObject(ShownEventCard);
#pragma warning restore 618

                MoveCardToGraveyard(card, leaderCardStack.Graveyard);
#pragma warning disable 618
                ShownEventCard = NetworkInstanceId.Invalid;
#pragma warning restore 618
            }
        }

#pragma warning disable 618

        [Command]
        public void CmdAcceptTradingOffer(TradingOffer offer)
        {
            int index = Diplomacy.PlayerRelationships.Find(offer.Owner);
            if (index != -1 &&
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

                if (Resources.HasResources(playerResourcesRequired) &&
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
        [Command]
        public void CmdRemoveResource(NetworkInstanceId playerId, ResourceType type)
        {
            Assert.IsTrue(isServer, "Only run CmdRemoveResource on server");
            var resources = NetworkServer.FindLocalObject(playerId).GetComponent<Resources>();
            var resourcesToRemove = new ResourceInfo[]
            {
                new ResourceInfo{Type = type, Amount = -1}
            };
            resources.AddResources(resourcesToRemove);

            RemovedResources++;
            if (RemovedResources == 2)
            {
                CurrentEnemyToRemoveFrom++;
                RemovedResources = 0;
                if (CurrentEnemyToRemoveFrom >= PlayersInWarWith.Count)
                {
                    State = PlayerState.CurrentPlayer;
                    RpcClearPlayersInWarWith();
                }
            }
        }
        
        [Command]
            public void CmdNextPlayerInWarWithOrEnd()
        {
            CurrentEnemyToRemoveFrom++;
            RemovedResources = 0;
            if (CurrentEnemyToRemoveFrom >= PlayersInWarWith.Count)
            {
                State = PlayerState.CurrentPlayer;
                RpcClearPlayersInWarWith();
            }
        }
#pragma warning restore 618

        public void SelectLeaderCard()
        {
            State = PlayerState.LeaderCardSelection;
            foreach (var instance in HandCards)
            {
#pragma warning disable 618
                var card = NetworkServer.FindLocalObject(instance.Id).GetComponent<CardDisplay>().Card;
#pragma warning restore 618

                if (card.Type == CardType.Leader)
                {
                    return;
                }
            }

            GameManager.RequestLeaderCardsForSelection(netId);
        }

        [Command]
        private void CmdAddLeaderCardFromHand(NetworkInstanceId cardId)
        {
            AddLeaderCard(cardId);
            HandCards.Remove(new NetworkInstance { Id = cardId });
            GameManager.SelectedLeaderCard(netId);
        }

        [Command]
        private void CmdBuildBuilding(string buildingName, NetworkInstanceId regionId, bool forFree)
        {
            var buildingPrefab = GameManager.GetBuilding(buildingName);
            if (buildingPrefab.Type == buildingName)
            {
                if (buildingName == "Kindergarden")
                {
                    HasKindergarden = true;
                }

                var building = Instantiate(buildingPrefab);
                var region = NetworkServer.FindLocalObject(regionId).GetComponent<Region>();
                building.transform.SetParent(region.transform);
                //NOTE: Translate this a little bit, so that the building does not
                //overlap with mask that indicates which player ownes the region.
                building.transform.localPosition = new Vector3(0, 0, -0.01f);
                NetworkServer.Spawn(building.gameObject);
                region.RpcSetBuilding(building.netId);
                if (!forFree)
                {
                    Resources.ApplyConstructionCosts(building.ConstructionCosts, TechnologyConstructionCostReduction);
                    TechnologyConstructionCostReduction = 0;
                }
                VictoryPoints.ApplyVictoryPoints(building.GainedVictoryPoints);
                if (buildingName == "Opera")
                {
                    VictoryPoints.AddCulture(OperaPointsBuff, false);
                }
            }
        }

        /// <summary>
        /// Request the server to add a region to its own terretory
        /// </summary>
        /// <param name="regionId"></param>
#pragma warning disable 618
        [Command]
        private void CmdRequestConquerRegion(NetworkInstanceId regionId, bool forFree)
#pragma warning restore 618
        {
#pragma warning disable 618
            var region = NetworkServer.FindLocalObject(regionId).GetComponent<Region>();
#pragma warning restore 618
            if (forFree)
            {
                RpcAddRegion(regionId);
                region.RpcSetOwner(netId);
                if (region.Building)
                {
                    VictoryPoints.ApplyVictoryPoints(region.Building.GainedVictoryPoints);
                }
            }
            uint price = (uint)(3 * NextRegionCostFactor);
            if (Resources.Power >= price)
            {
                NextRegionCostFactor = 1;
                Resources.Power -= (int)price;
                RpcAddRegion(regionId);
                region.RpcSetOwner(netId);
                if (region.Building)
                {
                    VictoryPoints.ApplyVictoryPoints(region.Building.GainedVictoryPoints);
                }
            }

            if (BuildingsToBuild.Count > 0)
            {
                CmdBuildBuilding(BuildingsToBuild[0], regionId, true);
                BuildingsToBuild.RemoveAt(0);
            }
        }

        public void TryBuyTechnologyCard()
        {
            if (Resources.Technology >= 3)
            {
                CmdRequestTechnologyCardPurchase();
            }
        }

#pragma warning disable 618
        [Command]
        private void CmdRequestTechnologyCardPurchase()
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "CmdRequestTechnologyCardPurchase is only allowed to" +
                          " get called serverside");

            if (GameManager.RequestTechnologyCard(netId))
            {
                Resources.Technology -= 3;
            }
        }

#pragma warning disable 618
        public void AddLeaderCard(NetworkInstanceId cardId)
#pragma warning restore 618
        {
            //Adding draw card sound
            GameObject audio = GameObject.Find("CardDraw");
            audio.GetComponent<AudioSource>().Play();

            State = PlayerState.Idle;
#pragma warning disable 618
            var card = NetworkServer.FindLocalObject(cardId);
#pragma warning restore 618
            string leaderCardGroup = card.GetComponent<CardDisplay>().Card.className;
            if (LeaderCardsOfGroup.ContainsKey(leaderCardGroup))
            {
                LeaderCardsOfGroup[leaderCardGroup]++;
                // HACK: Ignoring the amount of cards of the group the player has
                // only works because each card raises the buff linear. If this changes
                //at some point of the balancing process this has to be revisited
                switch (leaderCardGroup)
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
                            Resources.ConstructionMaterialOrTechnologyPoints++;
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

            foreach (var effect in card.GetComponents<CardEffect>())
            {
                if (effect.IsLeaderEffect)
                {
                    effect.Execute(this);
                }
            }

            card.GetComponent<CardDisplay>().IsHoverd = false;
            card.GetComponent<CardDisplay>().IsMoving = false;
            card.transform.position = LeaderCardAnchor.position;
            RpcAddLeaderCard(cardId);
        }

        public void AddCardToHand(CardDisplay card)
        {
            //Adding draw card sound
            GameObject audio = GameObject.Find("CardDraw");
            audio.GetComponent<AudioSource>().Play();

            if (card.Card.Type == CardType.Event)
            {
                card.transform.position = EventCardSlot.position;
                card.transform.LookAt(transform);
                card.transform.Rotate(0, 180, 0);
                card.OldPosition = EventCardSlot.position;
                ShownEventCard = card.netId;
#pragma warning disable 618
                CmdPlayCard(card.GetComponent<NetworkIdentity>().netId);
#pragma warning restore 618
            }
            else
            {
                card.transform.position = transform.position + (transform.forward * 8);
                card.transform.LookAt(transform);
                card.transform.Rotate(0, 180, 0);
                card.transform.position += transform.right * (-1.5f + (HandCards.Count * 2.5f));
                card.OldPosition = card.transform.position;
#pragma warning disable 618
                HandCards.Add(new NetworkInstance { Id = card.netId });
#pragma warning restore 618
            }
        }

#pragma warning disable 618
        [Command]
        void CmdSetPlayerData(string name, Color color)
#pragma warning restore 618
        {
            PlayerName = name;
            PlayerColor = color;
        }

#pragma warning disable 618
        [Command]
        private void CmdPlayCard(NetworkInstanceId cardId)
#pragma warning restore 618
        {
            bool CanEffectsGetExecuted(List<CardEffect> cardEffects)
            {
                foreach (var effect in cardEffects)
                {
                    if (!effect.IsExecutionPossible(this))
                    {
                        return false;
                    }
                }

                return true;
            }

            Assert.IsTrue(isServer, "Only call CmdPlayCard on the Server");
#pragma warning disable 618
            var cardGO = NetworkServer.FindLocalObject(cardId);
#pragma warning restore 618
            var card = cardGO.GetComponent<CardDisplay>().Card;
            cardGO.GetComponent<CardDisplay>().IsHoverd = false;
            cardGO.GetComponent<CardDisplay>().IsMoving = false;
            var effects = new List<CardEffect>();
            Transform parent = null;
            if (card.Type == CardType.Technology)
            {
                var technologyCardStack =
                    GameObject.Find("TechnologyCards").GetComponent<TechnologyCardStack>();

                effects.AddRange(cardGO.GetComponents<CardEffect>());
                parent = technologyCardStack.Graveyard;
            }
            else if (card.Type == CardType.Leader)
            {
                var leaderCardStack =
                    GameObject.Find("LeaderCards").GetComponent<MainCardStack>();

                parent = leaderCardStack.Graveyard;
                foreach (var effect in cardGO.GetComponents<CardEffect>())
                {
                    if (!effect.IsLeaderEffect)
                    {
                        effects.Add(effect);
                    }
                }
            }
            else if (card.Type == CardType.OldMaid)
            {
                var leaderCardStack =
                    GameObject.Find("LeaderCards").GetComponent<MainCardStack>();

                parent = leaderCardStack.Graveyard;
                effects.AddRange(cardGO.GetComponents<CardEffect>());
            }
            else if (card.Type == CardType.Event)
            {
                var leaderCardStack =
                    GameObject.Find("LeaderCards").GetComponent<MainCardStack>();

                parent = leaderCardStack.Graveyard;
                effects.AddRange(cardGO.GetComponents<CardEffect>());

                if (CanEffectsGetExecuted(effects))
                {
                    bool isSyncron = true;
                    foreach (var effect in effects)
                    {
                        if (effect.IsAsyncron)
                        {
                            isSyncron = false;
                            break;
                        }
                    }

                    foreach (var effect in effects)
                    {
                        effect.Execute(this);
                    }
                    HandCards.Remove(new NetworkInstance { Id = cardId });

                    if (isSyncron)
                    {
                        StartCoroutine(ShowEventCard(cardGO, parent));
                    }
                    else
                    {
                        cardGO.transform.position = EventCardSlot.position;
                        cardGO.transform.LookAt(transform);
                        cardGO.transform.Rotate(0, 180, 0);
                        cardGO.GetComponent<CardDisplay>().OldPosition = EventCardSlot.position;
#pragma warning disable 618
                        ShownEventCard = cardGO.GetComponent<NetworkIdentity>().netId;
#pragma warning restore 618
                    }
                }
                else
                {
                    MoveCardToGraveyard(cardGO, parent);
                }

                return;
            }

            if (CanEffectsGetExecuted(effects))
            {
                foreach (var effect in effects)
                {
                    effect.Execute(this);
                }

                MoveCardToGraveyard(cardGO, parent);

                //Adding draw card sound
                GameObject audio = GameObject.Find("CardPlay");
                audio.GetComponent<AudioSource>().Play();

                HandCards.Remove(new NetworkInstance { Id = cardId });
            }
        }

        void MoveCardToGraveyard(GameObject cardGO, Transform parent)
        {
            cardGO.transform.SetParent(parent);
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.Euler(90, 90, 0);
        }

        IEnumerator ShowEventCard(GameObject card, Transform parent)
        {
            // CLEANUP: Maybe here is code duplicaiton with MoveEventCard
            yield return new WaitForSeconds(4);
            //Adding draw card sound
            GameObject audio = GameObject.Find("CardPlay");
            audio.GetComponent<AudioSource>().Play();
            MoveCardToGraveyard(card, parent);
#pragma warning disable 618
            ShownEventCard = NetworkInstanceId.Invalid;
#pragma warning restore
        }

        /// <summary>
        /// Requests the server to register the player at the <see cref="GameManager"/>
        /// </summary>
#pragma warning disable 618
        [Command]
        private void CmdRegisterPlayer(NetworkInstanceId gameManagerId)
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
        [Command]
        public void CmdEndTurn()
#pragma warning restore 618
        {
            if (isServer)
            {
                State = PlayerState.Idle;
                GameManager.EndPlayerTurn();
            }
            RpcProcessAllGUIs();
            ProcessGUINextFrame();
        }

#pragma warning disable 618
        [Command]
        private void CmdChooseCard(NetworkInstanceId netId)
#pragma warning restore 618
        {
            GameManager.RequestLeaderCard(this.netId, netId);
        }

        /// <summary>
        /// Makes the player the current player to take its turn
        /// </summary>
        public void MakeCurrentPlayer()
        {
            if (isServer)
            {
                State = PlayerState.CurrentPlayer;
                Diplomacy.HasDoneDiplomacyAction = false;
                GameManager.RequestMainCardStackTopCard(netId);
                Resources.ApplyPerRoundValues();

                // TODO: In case an event card is drawn this overwrites the state of the event find a way to fix this
                var playersInWarWith = Diplomacy.PlayerRelationships.FindAll(RelationshipState.War);
                if (playersInWarWith.Count > 0)
                {
                    State = PlayerState.EnemyResourceRemoval;
                    CurrentEnemyToRemoveFrom = 0;
                    foreach (var enemy in playersInWarWith)
                    {
                        RpcAddPlayerInWarWith(enemy.PartnerId);
                    }
                }

                int resourcesToGenerate = RandomResourcesOnTurnStart;
                resourcesToGenerate += Diplomacy.PlayerRelationships.FindAll(RelationshipState.TradingAgreement).Count;
                resourcesToGenerate += Diplomacy.PlayerRelationships.FindAll(RelationshipState.Alliance).Count * 2;
                if (HasKindergarden)
                {
                    resourcesToGenerate += KindergardenResources;
                }

                for (int i = 0; i < resourcesToGenerate; i++)
                {
                    var resource = (ResourceType)Random.Range(0, (int)ResourceType.Count);
                    switch (resource)
                    {
                        case ResourceType.ConstructionMaterial: { Resources.ConstructionMaterial++; break; }
                        case ResourceType.Power: { Resources.Power++; break; }
                        case ResourceType.Technology: { Resources.Technology++; break; }
                    }
                }

                if (GetEconomyPointsThisRound)
                {
                    VictoryPoints.AddEconomy(EconomyOrSciencePoints, false);
                }
                else
                {
                    VictoryPoints.AddScience(EconomyOrSciencePoints, false);
                }
                GetEconomyPointsThisRound = !GetEconomyPointsThisRound;

                Resources.UpdateEffects();

                foreach (var region in OwnedRegions.Values)
                {
                    if (region.Building)
                    {
                        if (BuildingResourcesBuffs.ContainsKey(region.Building.Type))
                        {
                            Resources.AddResources(BuildingResourcesBuffs[region.Building.Type].ToArray());
                        }
                        if (BuildingVictoryPointsBuffs.ContainsKey(region.Building.Type))
                        {
                            VictoryPoints.ApplyVictoryPoints(BuildingVictoryPointsBuffs[region.Building.Type].ToArray());
                        }
                        Resources.AddResources(region.Building.GeneratedResources);
                        if (region.Building.Type == "Opera")
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
            if (isServer)
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
        public Dictionary<string, List<ResourceInfo>> BuildingResourcesBuffs = new Dictionary<string, List<ResourceInfo>>();
        public Dictionary<string, List<Building.VictoryPoints>> BuildingVictoryPointsBuffs = new Dictionary<string, List<Building.VictoryPoints>>();
        #endregion
        const int LEFT_MOUSEBUTTON = 0;
        const int RIGHT_MOUSEBUTTON = 1;

        public override string ToString()
        {
            return $"{PlayerName} ({netId})";
        }
    }
}
