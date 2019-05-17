using UnityEngine;

namespace cdv
{
    public sealed class GrantRandomResourcesEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            owner.RandomResourcesOnTurnStart += Amount;
        }

        [SerializeField] int Amount;
    }
}