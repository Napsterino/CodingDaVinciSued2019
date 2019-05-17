using System.Collections.Generic;
using UnityEngine;

namespace cdv
{
    public sealed class ExtraResourceFromBuildingEffect : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            if(owner.BuildingResourcesBuffs.ContainsKey(BuildingType))
            {
                owner.BuildingResourcesBuffs[BuildingType].Add(new ResourceInfo { Type = Type, Amount = Amount });
            }
            else
            {
                owner.BuildingResourcesBuffs.Add(BuildingType, new List<ResourceInfo>());
                owner.BuildingResourcesBuffs[BuildingType].Add(new ResourceInfo { Type = Type, Amount = Amount });
            }
        }

        [SerializeField] string BuildingType;
        [SerializeField] ResourceType Type;
        [SerializeField] int Amount;
    }
}