using UnityEngine;

namespace cdv
{
    public sealed class ReduceRegionCostEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            owner.NextRegionCostFactor = ReductionFactor;
        }

        [SerializeField] private float ReductionFactor = 1;
    }
}