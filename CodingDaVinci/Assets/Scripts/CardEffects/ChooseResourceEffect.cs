using UnityEngine;

namespace cdv
{
    public sealed class ChooseResourceEffect : CardEffect
    {
        public override bool IsAsyncron => true;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            owner.State = PlayerState.ResourceSelection;
            owner.SelectedResourceAmount = Amount;
        }

        [SerializeField] int Amount;
    }
}