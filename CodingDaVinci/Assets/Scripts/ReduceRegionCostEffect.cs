using UnityEngine;

namespace cdv
{
    public sealed class ReduceRegionCostEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            owner.NextRegionCostFactor = ReductionFactor;
        }

        [SerializeField] private float ReductionFactor = 1;
    }
}