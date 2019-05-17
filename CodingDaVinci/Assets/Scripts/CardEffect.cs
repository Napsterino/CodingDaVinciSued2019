using UnityEngine;

namespace cdv
{
    /// <summary>
    /// Base class for card effects. Inherit from it when making a new effect
    /// </summary>
    public abstract class CardEffect : MonoBehaviour
    {
        /// <summary>
        /// Wheter or not the effect will be executed immediately or not
        /// </summary>
        public abstract bool IsAsyncron { get; }

        /// <summary>
        /// Whether or not the effect can be executed in the current gamestate
        /// </summary>
        /// <returns>True if the effect can be executed, false otherwise</returns>
        public abstract bool IsExecutionPossible(Player executer);

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