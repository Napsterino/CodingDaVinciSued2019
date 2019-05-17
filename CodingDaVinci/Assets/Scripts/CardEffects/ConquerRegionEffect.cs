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
    
    public class ConquerRegionEffect : CardEffect
    {
        public override bool IsAsyncron => true;
        
        public override bool IsExecutionPossible(Player executor)
        {
            var allRegions = FindObjectsOfType<Region>();
            foreach(var region in allRegions)
            {
                if(FulfillsRegionRequirements(region, Requirements, executor))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        protected bool FulfillsRegionRequirements(Region region, ConquerRequirement[] requirements,
                                                  Player effectExecutor)
        {
            if(region.IsStartRegion())
            {
                return false;
            }
            
            foreach(var requirement in requirements)
            {
                switch(requirement)
                {
                    case ConquerRequirement.MustBeFree:
                    {
                        if(region.Owner != null)
                        {
                            return false;
                        }
                        break;
                    }
                    
                    case ConquerRequirement.MustBeNoSeaRegion:
                    {
                        if(region.HasSeaAccess())
                        {
                            return false;
                        }
                        break;
                    }
                    
                    case ConquerRequirement.MustBeOccupiedByOtherPlayer:
                    {
                        if(region.Owner == effectExecutor || region.Owner == null)
                        {
                            return false;
                        }
                        break;
                    }
                    
                    case ConquerRequirement.MustBeSeaRegion:
                    {
                        if(!region.HasSeaAccess())
                        {
                            return false;
                        }
                        break;
                    }
                    
                    case ConquerRequirement.MustHaveNoOccupiedNeighbours:
                    {
                        bool foundOwnedNeighbour = false;
                        foreach(var neighbour in region.NeighbourRegions)
                        {
                            if(neighbour.Owner != null)
                            {
                                foundOwnedNeighbour = true;
                                break;
                            }
                        }
                        
                        if(foundOwnedNeighbour)
                        {
                            return false;
                        }
                        break;
                    }
                    
                    default:
                    {
                        Debug.LogError($"{requirement} is not handled");
                        break;
                    }
                }
            }
            
            return true;
        }
        
        public override void Execute(Player owner)
        {
            if(IsExecutionPossible(owner))
            {
                SetPlayerToConquerRegion(owner);
            }
        }
        
        protected void SetPlayerToConquerRegion(Player player)
        {
            player.State = PlayerState.ConquerRegion;
            player.RegionGetsAddedToPlayer = RegionGetsAddedToPlayer;
            player.ConquerRequirements = new ConquerRequirements
            {
                Requirements = Requirements
            };
        }
        
        [SerializeField] protected ConquerRequirement[] Requirements;
        [SerializeField] bool RegionGetsAddedToPlayer = true;
    }
}