using UnityEngine;

namespace cdv
{
    public sealed class AddKindergardenResourcesEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            owner.KindergardenResources += Amount;
        }

        [SerializeField] int Amount;
    }
}