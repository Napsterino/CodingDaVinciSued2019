using UnityEngine;

namespace cdv
{
    public sealed class ConquerRegionAndBuildBuildingEffect : CardEffect
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
            owner.RegionGetsAddedToPlayer = true;
            owner.BuildingsToBuild.Add(Building);
            owner.ConquerRequirements = new ConquerRequirements
            {
                Requirements = Requirements
            };
        }

        [SerializeField] ConquerRequirement[] Requirements;
        [SerializeField] string Building;
    }
}