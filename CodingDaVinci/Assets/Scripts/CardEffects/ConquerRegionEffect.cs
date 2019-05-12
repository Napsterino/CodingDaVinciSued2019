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
        MustBeFree,
        MustBeOccupiedByOtherPlayer
    }

    public sealed class ConquerRegionEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            // TODO: Check here if certain Requirements can not be fulfilled.
            // If that is the case do not play the card at all
            foreach(var element in Requirements)
            {
                switch(element)
                {
                    case ConquerRequirement.MustBeOccupiedByOtherPlayer:
                    {
                        bool regionAvailable = false;
                        foreach(var player in owner.GameManager.Players)
                        {
                            if(player != owner)
                            {
                                foreach(var region in player.OwnedRegions.Values)
                                {
                                    if(!region.IsStartRegion())
                                    {
                                        regionAvailable = true;
                                        break;
                                    }
                                }
                                
                                if(regionAvailable)
                                {
                                    break;
                                }
                            }
                        }

                        if(!regionAvailable)
                        {
                            return;
                        }

                        break;
                    }
                }
            }

            owner.State = PlayerState.ConquerRegion;
            owner.ConquerRequirements = new ConquerRequirements
            {
                Requirements = Requirements
            };
        }

        [SerializeField] private ConquerRequirement[] Requirements;
    }
}