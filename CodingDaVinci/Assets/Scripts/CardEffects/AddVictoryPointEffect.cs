using UnityEngine;

namespace cdv
{
    /// <summary>
    /// Adds victory points to the player who played the card
    /// </summary>
    public sealed class AddVictoryPointEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        /// <summary>
        /// Implementation of <see cref="CardEffect.Execute(Player)"/>
        /// </summary>
        public override void Execute(Player owner)
        {
            switch(Category)
            {
                case VictoryPointCategory.Culture:
                {
                    owner.VictoryPoints.AddCulture(Amount, Permanent);
                    break;
                }

                case VictoryPointCategory.Economy:
                {
                    owner.VictoryPoints.AddEconomy(Amount, Permanent);
                    break;
                }

                case VictoryPointCategory.Science:
                {
                    owner.VictoryPoints.AddScience(Amount, Permanent);
                    break;
                }

                case VictoryPointCategory.Territory:
                {
                    owner.VictoryPoints.AddTerritory(Amount, Permanent);
                    break;
                }

                default:
                {
                    Debug.LogError($"{Category} is not handled");
                    break;
                }
            }
        }
          
        /// <summary>
        /// Category of victory points the points get applied to
        /// </summary>
        public VictoryPointCategory Category;

        /// <summary>
        /// Amount of points to add to the given category
        /// </summary>
        public int Amount;

        /// <summary>
        /// Whether or not these points are just for one round or until the end of the game.
        /// True: Unitl the end of the game. False: Until the end of the round.
        /// </summary>
        public bool Permanent;
    }
}