using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cdv
{
    public class CardDisplay : MonoBehaviour
    {

        public Card Card;
        [SerializeField] private Text nameText;
        [SerializeField] private Text className;
        [SerializeField] private Text leaderAbility;
        [SerializeField] private Text playAbility;
        [SerializeField] private Image artwork;

        void Start()
        {
            nameText.text = Card.name;
            className.text = Card.className;
            leaderAbility.text = Card.leaderAbility;
            playAbility.text = Card.playAbility;
            artwork.sprite = Card.artwork;
        }


    }

}
