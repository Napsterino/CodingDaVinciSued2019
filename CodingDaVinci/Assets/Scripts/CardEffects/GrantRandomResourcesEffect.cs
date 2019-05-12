using UnityEngine;

namespace cdv
{
    public sealed class GrantRandomResourcesEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            owner.RandomResourcesOnTurnStart += Amount;
        }

        [SerializeField] int Amount;
    }
}