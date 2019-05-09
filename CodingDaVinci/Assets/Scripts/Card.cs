using UnityEngine;

namespace cdv
{
    public enum CardType
    {
        Leader,
        Technology,
        Event,
        OldMaid
    }

    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject

    {
        public new string name;
        public string leaderAbility;
        public Sprite artwork;
        public string className;
        public string playAbility;
        public CardType Type;
    }

}
