using UnityEngine;

namespace cdv
{
    public struct ConquerRequirements
    {
        public ConquerRequirement[] Requirements;
    }

    public enum ConquerRequirement
    {
        MustBeSeaRegion,
        MustHaveNoOccupiedNeighbours,
        MustBeNoSeaRegion,
        MustBeFree
    }

    public sealed class ConquerRegionEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            owner.State = PlayerState.ConquerRegion;
            owner.ConquerRequirements = new ConquerRequirements
            {
                Requirements = Requirements
            };
        }

        [SerializeField] private ConquerRequirement[] Requirements;
    }
}