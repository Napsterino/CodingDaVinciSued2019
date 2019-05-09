using UnityEngine;

namespace cdv
{
    /// <summary>
    /// Base class for card effects. Inherit from it when making a new effect
    /// </summary>
    public abstract class CardEffect : MonoBehaviour
    {
        /// <summary>
        /// Executes the effect
        /// </summary>
        /// <param name="owner">player how played the card</param>
        public abstract void Execute(Player owner);

        /// <summary>
        /// In case the effect is placed on a leadercard this defines whether or not
        /// the effect is applied when the leader card is played or when it is locked in
        /// as leadercard. True means the effect belongs to the leader effects. False means
        /// it belongs to play effects.
        /// </summary>
        public bool IsLeaderEffect => m_IsLeaderEffect;
        [SerializeField] protected bool m_IsLeaderEffect;
    }
}