using UnityEngine;

namespace cdv
{
    public sealed class BuildFreeBuildingEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            owner.State = PlayerState.BuildingMode;
            owner.BuildingsToBuild.Add(Building.Type);
        }

        [SerializeField] Building Building;
    }
}