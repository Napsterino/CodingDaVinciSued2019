using UnityEngine;

namespace cdv
{
    public sealed class GetVictoryPointsForBuilding : CardEffect
    {
        public override bool IsAsyncron => false;

        public override bool IsExecutionPossible(Player executer)
        {
            return true;
        }

        public override void Execute(Player owner)
        {
            int amount = 0;
            foreach(var region in owner.OwnedRegions)
            {
                if(region.Value.Building && region.Value.Building.Type == BuildingName)
                {
                    amount += AmountPerBuilding;
                }
            }

            owner.VictoryPoints.ApplyVictoryPoints(new Building.VictoryPoints[]
            {
                new Building.VictoryPoints{Category = Type, Amount = amount}
            }, false);
        }
        
        [SerializeField] string BuildingName;
        [SerializeField] VictoryPointCategory Type;
        [SerializeField] int AmountPerBuilding;
    }
}