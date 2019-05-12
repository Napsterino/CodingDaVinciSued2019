using UnityEngine;

namespace cdv
{
    public sealed class AddKindergardenResources : CardEffect
    {
        public override void Execute(Player owner)
        {
            owner.KindergardenResources += Amount;
        }

        [SerializeField] int Amount;
    }
}