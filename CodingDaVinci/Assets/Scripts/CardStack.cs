using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace cdv
{
#pragma warning disable 618
    public class CardStack : NetworkBehaviour
#pragma warning restore 618
    {
        #region Client Code
        public override void PreStartClient()
        {
            base.PreStartClient();
            Cards = new List<CardDisplay>(20);
            Cards.AddRange(GetComponentsInChildren<CardDisplay>());
        }
        #endregion

        #region Server Code
        /// <summary>
        /// Takes the card from the top of the stack and assigns it to a player
        /// </summary>
        /// <param name="playerId">id of the player</param>
        /// <returns></returns>
#pragma warning disable 618
        public bool SendTopCardToPlayer(NetworkInstanceId playerId)
#pragma warning restore 618
        {
            Assert.IsTrue(isServer, "SentTopCardToPlayer is only allowed to get called serverside");
            if(Cards.Count > 0)
            {
#pragma warning disable 618
                var player = NetworkServer.FindLocalObject(playerId).GetComponent<Player>();
#pragma warning restore 618
                // NOTE: Its important to remove the card before adding it to the palyer hand
                // because if the card is an event card it might want to access the main stack
                // and than it would access itself
                Cards.RemoveAt(0);
                player.AddCardToHand(Cards[0]);
                return true;
            }
            else
            {
                return false;
            }
        }

        // TODO: Maybe make a somewhat smarter algorithm it feels like the cards that get
        // drawn arent distributed very well
        public void Shuffle(int iterations)
        {
            Assert.IsTrue(isServer, "Shuffle is only allowed to get called serverside");
            if(Cards.Count >= 2)
            {
                for(int i = 0; i < iterations; i++)
                {
                    int indexOne = Random.Range(0, Cards.Count);
                    int indexTwo = Random.Range(0, Cards.Count);
                    while(indexOne == indexTwo)
                    {
                        indexTwo = Random.Range(0, Cards.Count);
                    }

                    var tmp = Cards[indexOne];
                    Cards[indexOne] = Cards[indexTwo];
                    Cards[indexTwo] = tmp;
                }
            }
        }

        protected List<CardDisplay> Cards;
        #endregion

        #region Shared Code
        public Transform Graveyard => m_Graveyard;
        [SerializeField] Transform m_Graveyard;
        #endregion
    }
}