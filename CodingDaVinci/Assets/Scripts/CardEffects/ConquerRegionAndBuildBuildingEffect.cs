using UnityEngine;

namespace cdv
{
    public sealed class ConquerRegionAndBuildBuildingEffect : ConquerRegionEffect
    {
        public override bool IsAsyncron => true;
        
        public override bool IsExecutionPossible(Player executor)
        {
            var allRegions = FindObjectsOfType<Region>();
            foreach(var region in allRegions)
            {
                if(FulfillsRegionRequirements(region, Requirements, executor) &&
                   region.Building == null)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public override void Execute(Player owner)
        {
            if(IsExecutionPossible(owner))
            {
                SetPlayerToConquerRegion(owner);
                owner.BuildingsToBuild.Add(Building);
            }
        }
        
        [SerializeField] string Building;
    }
}