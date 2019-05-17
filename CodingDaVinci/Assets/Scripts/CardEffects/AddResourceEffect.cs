using UnityEngine;

namespace cdv
{
    /// <summary>
    /// Adds a resource to the player who played the card with this effect
    /// </summary>
    public sealed class AddResourceEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        /// <summary>
        /// Implementation of <see cref="CardEffect.Execute(Player)"/>
        /// </summary>
        /// <param name="owner"></param>
        public override void Execute(Player owner)
        {
            switch(Resource)
            {
                case ResourceType.ConstructionMaterial:
                {
                    owner.Resources.AddConstructionMaterial(Amount, Permanent);
                    break;
                }

                case ResourceType.Power:
                {
                    owner.Resources.AddPower(Amount, Permanent);
                    break;
                }

                case ResourceType.Technology:
                {
                    owner.Resources.AddTechnology(Amount, Permanent);
                    break;
                }
            }
        }

        /// <summary>
        /// Type of resource to add to
        /// </summary>
        [SerializeField] ResourceType Resource;

        /// <summary>
        /// Amount of resources added to this
        /// </summary>
        [SerializeField] int Amount;

        /// <summary>
        /// Whether or not this effect gets applied for each turn
        /// </summary>
        [SerializeField] bool Permanent;
    }
}