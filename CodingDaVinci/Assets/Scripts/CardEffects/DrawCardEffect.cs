using UnityEngine;

namespace cdv
{
    public enum CardStackType
    {
        MainStack,
        TechnologyStack
    }

    /// <summary>
    /// Draws another card from the top of the main card stack
    /// </summary>
    public sealed class DrawCardEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            if(StackType == CardStackType.MainStack)
            {
                for(int i = 0; i < Amount; i++)
                {
                    owner.GameManager.RequestMainCardStackTopCard(owner.netId);
                }
            }
            else if(StackType == CardStackType.TechnologyStack)
            {
                for(int i = 0; i < Amount; i++)
                {
                    owner.GameManager.RequestTechnologyCard(owner.netId);
                }
            }
            
        }

        [SerializeField] CardStackType StackType;
        [SerializeField] int Amount;
    }
}