using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
    public sealed class MainCardStack : CardStack
    {
        #region Client Code
#pragma warning disable 618
        [ClientRpc] void RpcClearCardsHoldedByPlayer(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isClient, "Only run RpcClearCardsHoldedByPlayer on Client");
            CardsHoldedByPlayer[playerId].Clear();
        }

#pragma warning disable 618
        [ClientRpc] void RpcAddCard(NetworkInstanceId playerId, NetworkInstanceId cardId)
#pragma warning restore 618
        {
#pragma warning disable 618
            var card = ClientScene.FindLocalObject(cardId).GetComponent<CardDisplay>();
#pragma warning restore 618
            if(CardsHoldedByPlayer.TryGetValue(playerId, out var cards))
            {
                cards.Add(card);
            }
            else
            {
                CardsHoldedByPlayer.Add(playerId, new List<CardDisplay>(3));
                CardsHoldedByPlayer[playerId].Add(card);
            }
        }
        #endregion

        #region Server Code
#pragma warning disable 618
        public void SendCardToPlayer(NetworkInstanceId cardId, NetworkInstanceId playerId)
#pragma warning restore 618
        {
#pragma warning disable 618
            var cardRequestedByPlayer = NetworkServer.FindLocalObject(cardId).GetComponent<CardDisplay>();
#pragma warning restore 618
            foreach(var card in CardsHoldedByPlayer[playerId])
            {
                if(card != cardRequestedByPlayer)
                {
                    card.IsHoverd = false;
                    card.IsMoving = false;
                    MoveBackOnStack(card);
                }
            }
            RpcClearCardsHoldedByPlayer(playerId);
        }

        private void MoveBackOnStack(CardDisplay card)
        {
            card.transform.position = new Vector3(0, -2, 0);
            card.transform.rotation = Quaternion.Euler(270, 0, 0);
            Cards.Add(card);
        }

        /// <summary>
        /// Requests leader cards from the MainCardStack inorder to choose one of them
        /// </summary>
        /// <param name="netId">player who request the leader cards</param>
        /// <param name="count">amount of leader cards the player requests</param>
#pragma warning disable 618
        public void RequestLeaderCardSelection(NetworkInstanceId netId, int count)
#pragma warning restore 618
        {
#pragma warning disable 618
            var player = NetworkServer.FindLocalObject(netId);
#pragma warning restore 618
            player.GetComponent<Player>().State = PlayerState.LeaderCardSelection;
            float x = -3f;
            for(int i = 0; i < count; i++)
            {
                var card = Cards.Find(element => element.Card.Type == CardType.Leader);
                Cards.Remove(card);
                RpcAddCard(netId, card.netId);
                card.transform.position = player.transform.position + player.transform.forward * 6f;
                card.transform.LookAt(player.transform);
                card.transform.Rotate(0, 180, 0);
                card.transform.Translate(0, 3.25f, 0);
                card.transform.position += player.transform.right * x;
                card.OldPosition = card.transform.position;
                x += 3f;
            }
        }
        #endregion

        #region Shared Code
#pragma warning disable 618
        public Dictionary<NetworkInstanceId, List<CardDisplay>> CardsHoldedByPlayer = new Dictionary<NetworkInstanceId, List<CardDisplay>>();
#pragma warning restore 618
        #endregion

    }
}