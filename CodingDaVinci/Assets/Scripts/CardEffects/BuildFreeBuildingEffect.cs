using UnityEngine;

namespace cdv
{
    public sealed class BuildFreeBuildingEffect : CardEffect
    {
        public override bool IsAsyncron => true;

        public override bool IsExecutionPossible(Player executer)
        {
            foreach(var region in executer.OwnedRegions)
            {
                if(region.Value.Building == null &&
                    region.Value.FulFillsRequirements(Building.ConstructionRequirements))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Execute(Player owner)
        {
            owner.State = PlayerState.BuildingMode;
            owner.BuildingsToBuild.Add(Building.Type);
        }

        [SerializeField] Building Building;
    }
}