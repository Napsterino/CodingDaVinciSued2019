using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    /// <summary>
    /// GameManager controling the game loop
    /// </summary>
#pragma warning disable 618
    public sealed class GameManager : NetworkBehaviour
#pragma warning restore 618
    {
        #region Client Code
#pragma warning disable 618
        [ClientRpc] private void RpcRegisterPlayer(NetworkInstanceId playerId)
#pragma warning restore 618
        {
#pragma warning disable 618
            var player = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
#pragma warning restore 618
            m_Players.Add(player);
        }

        /// <summary>
        /// Finds players that were already registered before a player joined the game
        /// and adds their local versions to its local GameManager
        /// </summary>
        /// <param name="playerIds"></param>
#pragma warning disable 618
        [ClientRpc] private void RpcFindAllAlreadyRegisteredPlayers(NetworkInstanceId[] playerIds)
#pragma warning restore 618
        {
            foreach(var id in playerIds)
            {
                if(m_Players.Find(x => x.netId == id) == null)
                {
#pragma warning disable 618
                    m_Players.Add(ClientScene.FindLocalObject(id).GetComponent<Player>());
#pragma warning restore 618
                }
            }
        }

        public IReadOnlyCollection<Building> Buildings => Array.AsReadOnly(m_Buildings);
        [SerializeField] private Building[] m_Buildings;
        [SerializeField] private Building[] SpecialBuildings;
        #endregion

        #region Shared Code
        public Building GetBuilding(string type)
        {
            foreach(var building in m_Buildings)
            {
                if(building.Type == type)
                {
                    return building;
                }
            }

            foreach(var building in SpecialBuildings)
            {
                if(building.Type == type)
                {
                    return building;
                }
            }

            return null;
        }

        public override void PreStartClient()
        {
            base.PreStartClient();
            var go = GameObject.Find("LeaderCards");
            MainCardStack = go?.GetComponent<MainCardStack>();
            go = GameObject.Find("TechnologyCards");
            TechnologyCardStack = go?.GetComponent<TechnologyCardStack>();

            foreach(var building in Buildings)
            {
#pragma warning disable 618
                ClientScene.RegisterPrefab(building.gameObject);
#pragma warning restore 618
            }

            foreach(var building in SpecialBuildings)
            {
#pragma warning disable 618
                ClientScene.RegisterPrefab(building.gameObject);
#pragma warning restore 618
            }
        }
        
        public MainCardStack MainCardStack;
        private TechnologyCardStack TechnologyCardStack;
        public IReadOnlyCollection<Player> Players => m_Players;
        public List<Player> m_Players = new List<Player>(PLAYER_COUNT);
        public int Round => m_Round;
#pragma warning disable 618
        [SyncVar] private int m_Round = 0;
#pragma warning restore 618
        #endregion

        #region Server Code
        /// <summary>
        /// Tells the GameManager that a certain player has selected a leader card
        /// </summary>
        /// <param name="playerId">player who selected a leader card</param>
#pragma warning disable 618
        public Player SelectedLeaderCard(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only call SelectedLeaderCard on the server");
            var player = m_Players.Find(x => x.netId.Value == playerId.Value);
            HasSelectedLeaderCard[player] = true;
            return player;
        }

        /// <summary>
        /// Request from the player to get a card as leader card that is owned by the
        /// cardstack until this moment
        /// </summary>
        /// <param name="playerId">player requesting a card</param>
        /// <param name="card">card the player requests</param>
#pragma warning disable 618
        public void RequestLeaderCard(NetworkInstanceId playerId, NetworkInstanceId card)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only call RequestLeaderCard on the server");
            MainCardStack.SendCardToPlayer(card, playerId);
            var player = SelectedLeaderCard(playerId);
            player.AddLeaderCard(card);
        }

        private void Update()
        {
            if(isServer)
            {
                switch(CurrentState)
                {
                    case State.PlayerRegistration:
                    {
                        if(ConnectedAuthorities == PLAYER_COUNT)
                        {
                            MainCardStack.Shuffle(50);
                            CurrentPlayer = UnityEngine.Random.Range(0, m_Players.Count);
                            int startPositionIndex = UnityEngine.Random.Range(0, PlayerSpawns.Length);

                            ResourceInfo[] startResources = new ResourceInfo[]
                            {
                                new ResourceInfo{ Type = ResourceType.ConstructionMaterial, Amount = 4 },
                                new ResourceInfo{ Type = ResourceType.Power, Amount = 4 },
                                new ResourceInfo{ Type = ResourceType.Technology, Amount = 4 }
                            };

                            int playerIndex = CurrentPlayer;
                            for(int i = 0; i < PLAYER_COUNT; i++)
                            {
                                m_Players[playerIndex].transform.position = PlayerSpawns[startPositionIndex].position;
                                m_Players[playerIndex].transform.rotation = PlayerSpawns[startPositionIndex].rotation;
                                m_Players[playerIndex].RpcSetCamera(PlayerSpawns[startPositionIndex].position, PlayerSpawns[startPositionIndex].rotation);
                                StartRegions[startPositionIndex].RpcSetOwner(m_Players[playerIndex].netId);
                                m_Players[playerIndex].RpcAddRegion(StartRegions[startPositionIndex].netId);

                                MainCardStack.RequestLeaderCardSelection(
                                    m_Players[playerIndex].netId, 3);
                                m_Players[playerIndex].Resources.AddResources(startResources);

                                playerIndex = (playerIndex + 1) % PLAYER_COUNT;
                                startPositionIndex = (startPositionIndex + 1) % PlayerSpawns.Length;
                            }

                            CurrentState = State.LeaderCardSelection;
                        }
                        break;
                    }

                    case State.LeaderCardSelection:
                    {
                        // REFACTOR: Maybe check this only when a player selected a card
                        foreach(bool leaderCardSelected in HasSelectedLeaderCard.Values)
                        {
                            if(!leaderCardSelected)
                            {
                                return;
                            }
                        }

                        MainCardStack.Shuffle(50);
                        TechnologyCardStack.Shuffle(50);
                        FirstPlayerOfCurrentRound = CurrentPlayer;
                        CurrentPlayerMark.position = m_Players[CurrentPlayer].CurrentPlayerMarkPosition();
                        ChangeCurrentPlayer();
                        CurrentState = State.Idle;
                        m_Round++;
                        break;
                    }

                    case State.Idle: break;
                }
            }
        }

        /// <summary>
        /// Request from a player to get the top card of the technology cardstack
        /// </summary>
        /// <param name="playerId">player who requests the card</param>
        /// <returns></returns>
#pragma warning disable 618
        public bool RequestTechnologyCard(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "RequestTechnologyCard is only allowed to get called" +
                " serverside");
            return TechnologyCardStack.SendTopCardToPlayer(playerId);
        }

        /// <summary>
        /// Request from a player to get the top card of the main cardstack
        /// </summary>
        /// <param name="playerId">player who request the card</param>
#pragma warning disable 618
        public void RequestMainCardStackTopCard(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            // REFACOTR: Maybe let the player request the card directly at the maincardStack
            Assert.IsTrue(isServer, "Call RequestMainCardStackTopCard only on the server");
            MainCardStack.SendTopCardToPlayer(playerId);
        }

        /// <summary>
        /// Registers a player object as player of the game
        /// </summary>
        /// <param name="player">player to register</param>
        public void RegisterPlayer(Player player)
        {
            if(isServer && !m_Players.Contains(player))
            {
                HasSelectedLeaderCard.Add(player, false);
#pragma warning disable 618
                var playerIds = new NetworkInstanceId[Players.Count];
#pragma warning restore 618
                for(int i = 0; i < playerIds.Length; i++)
                {
                    playerIds[i] = m_Players[i].netId;
                }
                RpcFindAllAlreadyRegisteredPlayers(playerIds);
                RpcRegisterPlayer(player.netId);
                ConnectedAuthorities++;
            }
        }

        /// <summary>
        /// Ends the turn of a player and starts the turn of the next on
        /// </summary>
        public void EndPlayerTurn()
        {
            Assert.IsTrue(isServer, "Only call EndPlayerTurn on the server");
            CurrentPlayer++;
            CurrentPlayer %= m_Players.Count;

            if(CurrentPlayer == FirstPlayerOfCurrentRound)
            {
                void DistributeVictoryPoints(VictoryPoints[] victoryPoints, uint first, uint second,
                    uint third, VictoryPointCategory category)
                {
                    int pointsFirst = 0;
                    int pointsSecond = 0;
                    int pointsThird = 0;
                    switch(category)
                    {
                        case VictoryPointCategory.Science:
                        {
                            pointsFirst = victoryPoints[0].Science;
                            pointsSecond = victoryPoints[1].Science;
                            pointsThird = victoryPoints[2].Science;
                            break;
                        }

                        case VictoryPointCategory.Territory:
                        {
                            pointsFirst = victoryPoints[0].Territory;
                            pointsSecond = victoryPoints[1].Territory;
                            pointsThird = victoryPoints[2].Territory;
                            break;
                        }

                        case VictoryPointCategory.Economy:
                        {
                            pointsFirst = victoryPoints[0].Economy;
                            pointsSecond = victoryPoints[1].Economy;
                            pointsThird = victoryPoints[2].Economy;
                            break;
                        }

                        case VictoryPointCategory.Culture:
                        {
                            pointsFirst = victoryPoints[0].Culture;
                            pointsSecond = victoryPoints[1].Culture;
                            pointsThird = victoryPoints[2].Culture;
                            break;
                        }

                        default:
                        {
                            Debug.LogError($"Category {category} is not handled");
                            break;
                        }
                    }

                    if(pointsFirst != pointsSecond)
                    {
                        victoryPoints[0].WinPoints += first;
                        if(pointsSecond != pointsThird)
                        {
                            victoryPoints[1].WinPoints += second;
                            victoryPoints[2].WinPoints += third;
                        }
                        else
                        {
                            victoryPoints[1].WinPoints += third;
                            victoryPoints[2].WinPoints += third;
                        }
                    }
                    else
                    {
                        victoryPoints[0].WinPoints += second;
                        victoryPoints[1].WinPoints += second;
                        victoryPoints[2].WinPoints += third;
                    }
                }

                var playerPoints = new VictoryPoints[PLAYER_COUNT];
                for(int i = 0; i < PLAYER_COUNT; i++)
                {
                    playerPoints[i] = m_Players[i].VictoryPoints;
                }

                Array.Sort(playerPoints, VictoryPoints.CompareSience);
                DistributeVictoryPoints(playerPoints, 4, 2, 1, VictoryPointCategory.Science);

                Array.Sort(playerPoints, VictoryPoints.CompareTerritory);
                DistributeVictoryPoints(playerPoints, 4, 2, 1, VictoryPointCategory.Territory);

                Array.Sort(playerPoints, VictoryPoints.CompareEconomy);
                DistributeVictoryPoints(playerPoints, 4, 2, 1, VictoryPointCategory.Economy);

                Array.Sort(playerPoints, VictoryPoints.CompareCulture);
                DistributeVictoryPoints(playerPoints, 8, 3, 1, VictoryPointCategory.Culture);

                foreach(var victoryPoints in playerPoints)
                {
                    victoryPoints.TempScience = 0;
                    victoryPoints.TempTerritory = 0;
                    victoryPoints.TempEconomy = 0;
                    victoryPoints.TempCulture = 0;
                }

                if(Round == 4 || Round == 8 || Round == 12)
                {
                    CurrentState = State.LeaderCardSelection;
                    var keys = new List<Player>(HasSelectedLeaderCard.Keys);
                    foreach(var key in keys)
                    {
                        HasSelectedLeaderCard[key] = false;
                    }

                    m_Round++;
                    foreach(var player in m_Players)
                    {
                        player.SelectLeaderCard();
                    }

                    return;
                }

                if(Round == 16)
                {
                    Array.Sort(playerPoints, VictoryPoints.CompareWinPoints);
                    if(playerPoints[0].WinPoints != playerPoints[1].WinPoints)
                    {
                        foreach(var player in m_Players)
                        {
                            player.SendWinner(playerPoints[0].netId);
                        }
                    }
                    else if(playerPoints[1].WinPoints == playerPoints[2].WinPoints)
                    {
                        // TODO: Implement when we can test with four players
                        /*if(playerPoints[2] == playerPoints[3])
                        {

                        }*/

#pragma warning disable 618
                        var winners = new NetworkInstanceId[]
#pragma warning restore 618
                        {
                            playerPoints[0].netId,
                            playerPoints[1].netId,
                            playerPoints[2].netId
                        };

                        foreach(var player in m_Players)
                        {
                            player.SendTie(winners);
                        }

                    }
                    else
                    {
#pragma warning disable 618
                        var winners = new NetworkInstanceId[]
#pragma warning restore 618
                        {
                            playerPoints[0].netId,
                            playerPoints[1].netId,
                        };

                        foreach(var player in m_Players)
                        {
                            player.SendTie(winners);
                        }
                    }
                        
                    return;
                }

                CurrentPlayer++;
                CurrentPlayer %= m_Players.Count;
                FirstPlayerOfCurrentRound = CurrentPlayer;
                CurrentPlayerMark.position = m_Players[CurrentPlayer].CurrentPlayerMarkPosition();
                m_Round++;
            }

            ChangeCurrentPlayer();
        }

        /// <summary>
        /// Informs all players about the new current player and gives him control
        /// </summary>
        private void ChangeCurrentPlayer()
        {
            m_Players[CurrentPlayer].MakeCurrentPlayer();
            Table.rotation = Quaternion.AngleAxis(m_Players[CurrentPlayer].transform.eulerAngles.y, Vector3.up);
            for (int i = 0; i < m_Players.Count; i++)
            {
                if (i != CurrentPlayer)
                {
                    m_Players[i].SetCurrentPlayerId(m_Players[CurrentPlayer].netId);
                }
            }
        }

        /// <summary>
        /// Requests to get leader cards for the leader card selection
        /// </summary>
        /// <param name="playerId">player requesting those cards</param>
#pragma warning disable 618
        public void RequestLeaderCardsForSelection(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            MainCardStack.RequestLeaderCardSelection(playerId, 2);
        }

        public Transform[] PlayerSpawns;
        [SerializeField] private Region[] StartRegions;
        [SerializeField] private Transform CurrentPlayerMark;
        public Dictionary<Player, bool> HasSelectedLeaderCard = new Dictionary<Player, bool>(PLAYER_COUNT);
        private int ConnectedAuthorities = 0;
        private int CurrentPlayer = 0;
        private int FirstPlayerOfCurrentRound = 0;
        private State CurrentState = State.PlayerRegistration;
        public StockExchange StockExchange => m_StockExchange;
        [SerializeField] private StockExchange m_StockExchange;
        [SerializeField] private Transform Table;
        public const int PLAYER_COUNT = 3;
        #endregion

        enum State
        {
            PlayerRegistration,
            LeaderCardSelection,
            Idle
        }
    }
} 