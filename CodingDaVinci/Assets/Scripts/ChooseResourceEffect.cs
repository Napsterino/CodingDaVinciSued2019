using UnityEngine;

namespace cdv
{
    public sealed class ChooseResourceEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            owner.State = PlayerState.ResourceSelection;
            owner.SelectedResourceAmount = Amount;
        }

        [SerializeField] int Amount;
    }
}