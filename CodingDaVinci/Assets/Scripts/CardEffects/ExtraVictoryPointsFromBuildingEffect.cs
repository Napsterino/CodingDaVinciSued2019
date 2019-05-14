using System.Collections.Generic;
using UnityEngine;

namespace cdv
{
    public sealed class ExtraVictoryPointsFromBuildingEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            if(owner.BuildingVictoryPointsBuffs.ContainsKey(BuildingType))
            {
                owner.BuildingVictoryPointsBuffs[BuildingType].Add(new Building.VictoryPoints { Category = Type, Amount = Amount });
            }
            else
            {
                owner.BuildingVictoryPointsBuffs.Add(BuildingType, new List<Building.VictoryPoints>());
                owner.BuildingVictoryPointsBuffs[BuildingType].Add(new Building.VictoryPoints { Category = Type, Amount = Amount });
            }
        }

        [SerializeField] string BuildingType;
        [SerializeField] VictoryPointCategory Type;
        [SerializeField] int Amount;
    }
}