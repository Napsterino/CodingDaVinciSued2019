using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public enum RelationshipState
    {
        War,
        TradingAgreement,
        Alliance,
        Peace
    }

    /// <summary>
    /// Handles all diplomatic actions between the players
    /// </summary>
#pragma warning disable 618
    public sealed class Diplomacy : NetworkBehaviour
#pragma warning restore 618
    {
        #region Shared Code
        #pragma warning disable 618
        public struct Relationship
        {
            public NetworkInstanceId PartnerId;
            public RelationshipState State; 
        };

        public class RelationShips : SyncListStruct<Relationship>
        {
            public int Find(NetworkInstanceId playerId)
            {
                for(int i = 0; i < Count; i++)
                {
                    if(this[i].PartnerId == playerId)
                    {
                        return i;
                    }
                }

                return -1;
            }

            public List<Relationship> FindAll(RelationshipState relationship)
            {
                var list = new List<Relationship>();
                foreach(var element in this)
                {
                    if(element.State == relationship)
                    {
                        list.Add(element);
                    }                    
                }
                return list;
            }
        }

        [SyncVar, HideInInspector] public bool HasDoneDiplomacyAction = false;
        [SyncVar, HideInInspector] public bool ReceivedRequest = false;
        [SyncVar] private NetworkInstanceId PlayerOfRequest;
        [SyncVar] private NetworkInstanceId SelectedPlayer = NetworkInstanceId.Invalid;
        [SyncVar] private RelationshipState RequestType;
        [SyncVar] private bool ReceivedWarAssistanceRequest = false;
        [SyncVar] private NetworkInstanceId Enemy;
        [HideInInspector] public RelationShips PlayerRelationships = new RelationShips();
#pragma warning restore 618
        #endregion

        #region Client Code
        /// <summary>
        /// Draws the ui for handling a peace request
        /// </summary>
        public void DrawRequestUI()
        {
            Assert.IsTrue(isLocalPlayer, "Only run DrawPeaceRequestUI on the LocalPlayer");
            if(ReceivedWarAssistanceRequest)
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 200));
                GUILayout.Label($"Ihr Bündnisparter {PlayerOfRequest} hat {Enemy} den Krieg erklärt. Möchten sie zu ihrem Bündnis stehen");
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Ja"))
                {
                    CmdSendDeclarationOfWar(Enemy);
#pragma warning disable 618
                    Enemy = NetworkInstanceId.Invalid;
                    SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                    ReceivedRequest = false;
                    ReceivedWarAssistanceRequest = false;
                }
                if(GUILayout.Button("Nein"))
                {
                    int index = PlayerRelationships.Find(PlayerOfRequest);
                    var relationship = PlayerRelationships[index];
                    relationship.State = RelationshipState.Peace;
                    PlayerRelationships[index] = relationship;
#pragma warning disable 618
                    var ally = NetworkServer.FindLocalObject(PlayerOfRequest).GetComponent<Diplomacy>();
#pragma warning restore 618
                    index = ally.PlayerRelationships.Find(netId);
                    relationship = ally.PlayerRelationships[index];
                    relationship.State = RelationshipState.Peace;
                    ally.PlayerRelationships[index] = relationship;
#pragma warning disable 618
                    Enemy = NetworkInstanceId.Invalid;
                    SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                    ReceivedWarAssistanceRequest = false;
                    ReceivedRequest = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            else
            {
                string request = "";
                switch(RequestType)
                {
                    case RelationshipState.Peace:
                    { request = "Frieden schließen"; break; }
                    case RelationshipState.TradingAgreement:
                    { request = "ein Handelsabkommen abschließen"; break; }
                }
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200));
                GUILayout.Label($"Spieler {PlayerOfRequest} möchte {request}");
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Akzeptieren"))
                {
                    CmdAcceptRequest(RequestType);
                }
                if(GUILayout.Button("Ablehenen"))
                {
                    CmdDeclineRequest();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        /// <summary>
        /// Draws the ui for taking basic diplomatic actions
        /// </summary>
        /// <returns>true if the ui shall be drawn again next frame, otherwise false</returns>
        public bool DrawUI()
        {
            Assert.IsTrue(isLocalPlayer, "Only run DrawUI on the localPlayer");
            bool keepDrawing = true;
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400));
#pragma warning disable 618
            if(SelectedPlayer != NetworkInstanceId.Invalid)
#pragma warning restore 618
            {
                int index = PlayerRelationships.Find(SelectedPlayer);
                if(index != -1)
                {
                    var relationship = PlayerRelationships[index];
                    if(relationship.State == RelationshipState.War)
                    {
                        if(GUILayout.Button("Frieden schließen"))
                        {
                            CmdSendPeaceRequest(SelectedPlayer);
                            keepDrawing = false;
#pragma warning disable 618
                            SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                        }
                    }
                    else if(relationship.State == RelationshipState.TradingAgreement)
                    {
                        if(GUILayout.Button("Bündnis eingehen"))
                        {
                            CmdRequestAlliance(SelectedPlayer);
                        }
                        if(GUILayout.Button("Handelsabkommen auflösen"))
                        {
                            CmdBreakTradingAgreement(SelectedPlayer);
                        }
                        if(GUILayout.Button("Krieg erklären"))
                        {
                            CmdSendDeclarationOfWar(SelectedPlayer);
                            keepDrawing = false;
#pragma warning disable 618
                            SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                        }
                    }
                    else if(relationship.State == RelationshipState.Alliance)
                    {
                        if(GUILayout.Button("Bündnis aufkündigen"))
                        {
                            CmdBreakAlliance(SelectedPlayer);
                        }
                        if(GUILayout.Button("Krieg erklären"))
                        {
                            CmdSendDeclarationOfWar(SelectedPlayer);
                        }
                    }
                    else if(relationship.State == RelationshipState.Peace)
                    {
                        if(GUILayout.Button("Handelsabkommen unterzeichnen"))
                        {
                            CmdRequestTradingAgreement(SelectedPlayer);
                        }
                        if(GUILayout.Button("Bündnis eingehen"))
                        {
                            CmdRequestAlliance(SelectedPlayer);
                        }
                        if(GUILayout.Button("Krieg erklären"))
                        {
                            CmdSendDeclarationOfWar(SelectedPlayer);
                            keepDrawing = false;
#pragma warning disable 618
                            SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                        }
                    }
                }
                else
                {
                    if(GUILayout.Button("Handelsabkommen unterzeichnen"))
                    {
                        CmdRequestTradingAgreement(SelectedPlayer);
                    }
                    if(GUILayout.Button("Bündnis eingehen"))
                    {
                        CmdRequestAlliance(SelectedPlayer);
                    }
                    if(GUILayout.Button("Krieg erklären"))
                    {
                        CmdSendDeclarationOfWar(SelectedPlayer);
                        keepDrawing = false;
#pragma warning disable 618
                        SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                    }
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                foreach(var player in GetComponent<Player>().GameManager.Players)
                {
                    if(player.netId != netId)
                    {
                        if(GUILayout.Button($"Player: {player.netId}"))
                        {
                            SelectedPlayer = player.netId;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            if(GUILayout.Button("Zurück"))
            {
#pragma warning disable 618
                if(SelectedPlayer != NetworkInstanceId.Invalid)
                {
                    SelectedPlayer = NetworkInstanceId.Invalid;
#pragma warning restore 618
                }
                else
                {
                    keepDrawing = false;
                }
            }
            GUILayout.EndArea();
            return keepDrawing;
        }
        #endregion


        #region Server Code
        // TODO: There are some other parts where this can be probably used.
        // Refactor the parts there
#pragma warning disable 618
        private void SetRelationship(NetworkInstanceId partnerId, RelationshipState newState)
#pragma warning restore 618
        {
            int index = PlayerRelationships.Find(partnerId);
            var relationship = PlayerRelationships[index];
            relationship.State = newState;
            PlayerRelationships[index] = relationship;
        }

#pragma warning disable 618
        [Command] public void CmdSetRelation(NetworkInstanceId partnerId, RelationshipState newState)
#pragma warning restore 618
        {
            SetRelationship(partnerId, newState);
        }

        /// <summary>
        /// Declares war to another player
        /// </summary>
        /// <param name="enemyId">player to declare war to</param>
#pragma warning disable 618
        [Command] private void CmdSendDeclarationOfWar(NetworkInstanceId enemyId)
        {
            Assert.IsTrue(isServer, "Only run CmdSendDeclarationOfWar on server");
            var enemy = NetworkServer.FindLocalObject(enemyId).GetComponent<Diplomacy>();
            int index = enemy.PlayerRelationships.Find(netId);
            if(index != -1)
            {
                var relationship = enemy.PlayerRelationships[index];
                relationship.State = RelationshipState.War;
                enemy.PlayerRelationships[index] = relationship;

                index = PlayerRelationships.Find(enemyId);
                relationship = PlayerRelationships[index];
                relationship.State = RelationshipState.War;
                PlayerRelationships[index] = relationship;
            }
            else
            {
                enemy.PlayerRelationships.Add(new Relationship
                {
                    PartnerId = netId,
                    State = RelationshipState.War
                });

                PlayerRelationships.Add(new Relationship
                {
                    PartnerId = enemyId,
                    State = RelationshipState.War
                });
            }

            foreach(var relationship in PlayerRelationships)
            {
                if(relationship.State == RelationshipState.Alliance && relationship.PartnerId != PlayerOfRequest)
                {
                    var ally = NetworkServer.FindLocalObject(relationship.PartnerId).GetComponent<Diplomacy>();
                    ally.RequestWarAssistance(netId, enemyId);
                }
            }

            foreach(var relationship in enemy.PlayerRelationships)
            {
                if(relationship.State == RelationshipState.Alliance)
                {
                    var ally = NetworkServer.FindLocalObject(relationship.PartnerId).GetComponent<Diplomacy>();
                    ally.RequestWarAssistance(enemyId, netId);
                }
            }

            HasDoneDiplomacyAction = true;
        }
#pragma warning restore 618

        /// <summary>
        /// Request to make peace with another player
        /// </summary>
        /// <param name="playerId">player who requests to make peace</param>
#pragma warning disable 618
        public void RequestPeace(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only call RequestPeace on the Server");
            ReceivedRequest = true;
            PlayerOfRequest = playerId;
            RequestType = RelationshipState.Peace;
        }

#pragma warning disable 618
        public void RequestWarAssistance(NetworkInstanceId allyId, NetworkInstanceId enemyId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only call RequestWarAssistance on server");
            ReceivedRequest = true;
            ReceivedWarAssistanceRequest = true;
            PlayerOfRequest = allyId;
            Enemy = enemyId;
        }

#pragma warning disable 618
        private void RequestTradingAgreement(NetworkInstanceId partnerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only call RequestTradingAgreement on the Server");
            ReceivedRequest = true;
            PlayerOfRequest = partnerId;
            RequestType = RelationshipState.TradingAgreement;
        }

        /// <summary>
        /// Declines a peace request
        /// </summary>
#pragma warning disable 618
        [Command] private void CmdDeclineRequest()
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only run CmdDeclineRequest on the server");
            ReceivedRequest = false;
#pragma warning disable 618
            PlayerOfRequest = NetworkInstanceId.Invalid;
#pragma warning restore 618
        }

        /// <summary>
        /// Accpets a made peace request
        /// </summary>
#pragma warning disable 618
        [Command] private void CmdAcceptRequest(RelationshipState type)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only run CmdAcceptRequest on the server");
#pragma warning disable 618
            var requester = NetworkServer.FindLocalObject(PlayerOfRequest).GetComponent<Diplomacy>();
#pragma warning restore 618
            int index = requester.PlayerRelationships.Find(netId);
            if(index != -1)
            {
                var relationship = requester.PlayerRelationships[index];
                relationship.State = type;
                requester.PlayerRelationships[index] = relationship;

                index = PlayerRelationships.Find(PlayerOfRequest);
                relationship = PlayerRelationships[index];
                relationship.State = type;
                PlayerRelationships[index] = relationship;
            }
            else
            {
                requester.PlayerRelationships.Add(new Relationship
                {
                    PartnerId = netId,
                    State = type
                });

                PlayerRelationships.Add(new Relationship
                {
                    PartnerId = PlayerOfRequest,
                    State = type
                });
            }

            ReceivedRequest = false;
#pragma warning disable 618
            PlayerOfRequest = NetworkInstanceId.Invalid;
#pragma warning restore 618
        }

        /// <summary>
        /// Command for sending a peace request to another player
        /// </summary>
        /// <param name="enemyId">player to send the request to</param>
#pragma warning disable 618
        [Command] private void CmdSendPeaceRequest(NetworkInstanceId enemyId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only run CmdSendPeaceRequest on server");
#pragma warning disable 618
            NetworkServer.FindLocalObject(enemyId).GetComponent<Diplomacy>().RequestPeace(netId);
#pragma warning restore 618
            HasDoneDiplomacyAction = true;
        }

        /// <summary>
        /// Ask another player to have a trading agreement wiht you
        /// </summary>
        /// <param name="partnerId">player to have a trading agreement with</param>
        #pragma warning disable 618
        [Command] private void CmdRequestTradingAgreement(NetworkInstanceId partnerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only run CmdRequestTradingAgreement on server");
#pragma warning disable 618
            NetworkServer.FindLocalObject(partnerId).GetComponent<Diplomacy>().RequestTradingAgreement(netId);
#pragma warning restore 618
            HasDoneDiplomacyAction = true;
        }

#pragma warning disable 618
        [Command] private void CmdBreakTradingAgreement(NetworkInstanceId partnerId)
#pragma warning restore 618
        {
#pragma warning disable 618
            var partner = NetworkServer.FindLocalObject(partnerId).GetComponent<Diplomacy>();
#pragma warning restore 618
            int index = partner.PlayerRelationships.Find(netId);
            if(index != -1)
            {
                var relationship = partner.PlayerRelationships[index];
                relationship.State = RelationshipState.Peace;
                partner.PlayerRelationships[index] = relationship;

                index = PlayerRelationships.Find(partnerId);
                relationship = PlayerRelationships[index];
                relationship.State = RelationshipState.Peace;
                PlayerRelationships[index] = relationship;
            }
            else
            {
                partner.PlayerRelationships.Add(new Relationship
                {
                    PartnerId = netId,
                    State = RelationshipState.Peace
                });

                PlayerRelationships.Add(new Relationship
                {
                    PartnerId = partnerId,
                    State = RelationshipState.Peace
                });
            }
            HasDoneDiplomacyAction = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerId"></param>
#pragma warning disable 618
        public void RequestAlliance(NetworkInstanceId partnerId)
#pragma warning disable 618
        {
            Assert.IsTrue(isServer, "Only call RequestTradingAgreement on the Server");
            ReceivedRequest = true;
            PlayerOfRequest = partnerId;
            RequestType = RelationshipState.Alliance;
        }

#pragma warning disable 618
        [Command] private void CmdRequestAlliance(NetworkInstanceId partnerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "Only run CmdRequestAlliance on server");
#pragma warning disable 618
            NetworkServer.FindLocalObject(partnerId).GetComponent<Diplomacy>().RequestAlliance(netId);
#pragma warning restore 618
            HasDoneDiplomacyAction = true;
        }

#pragma warning disable 618
        [Command] private void CmdBreakAlliance(NetworkInstanceId partnerId)
#pragma warning restore 618
        {
#pragma warning disable 618
            var partner = NetworkServer.FindLocalObject(partnerId).GetComponent<Diplomacy>();
#pragma warning restore 618
            int index = partner.PlayerRelationships.Find(netId);
            if(index != -1)
            {
                var relationship = partner.PlayerRelationships[index];
                relationship.State = RelationshipState.Peace;
                partner.PlayerRelationships[index] = relationship;

                index = PlayerRelationships.Find(partnerId);
                relationship = PlayerRelationships[index];
                relationship.State = RelationshipState.Peace;
                PlayerRelationships[index] = relationship;
            }
            else
            {
                partner.PlayerRelationships.Add(new Relationship
                {
                    PartnerId = netId,
                    State = RelationshipState.Peace
                });

                PlayerRelationships.Add(new Relationship
                {
                    PartnerId = partnerId,
                    State = RelationshipState.Peace
                });
            }
            HasDoneDiplomacyAction = true;
        }
        #endregion
    }
}