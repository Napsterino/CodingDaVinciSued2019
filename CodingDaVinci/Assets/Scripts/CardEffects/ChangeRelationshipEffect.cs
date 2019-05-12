using UnityEngine;

namespace cdv
{
    public sealed class ChangeRelationshipEffect : CardEffect
    {
        public override void Execute(Player owner)
        {
            bool oldStateExists = false;
            foreach(var player in owner.GameManager.Players)
            {
                for(int i = 0; i < player.Diplomacy.PlayerRelationships.Count; i++)
                {
                    var relationship = player.Diplomacy.PlayerRelationships[i];
                    if(relationship.State == CurrentState[0])
                    {
                        oldStateExists = true;
                        break;
                    }
                }

                if(oldStateExists)
                {
                    break;
                }
            }

            if(!oldStateExists)
            {
                return;
            }

            if(AllRelationships)
            {
                foreach(var player in owner.GameManager.Players)
                {
                    for(int i = 0; i < player.Diplomacy.PlayerRelationships.Count; i++)
                    {
                        var relationship = player.Diplomacy.PlayerRelationships[i];
                        if(relationship.State == CurrentState[0])
                        {
                            relationship.State = NewState;
                            player.Diplomacy.PlayerRelationships[i] = relationship;
                        }
                    }
                }
            }
            else
            {
                owner.State = PlayerState.ChangeRelationship;
                owner.OldRelationshipState = CurrentState[0];
                owner.NewRelationshipState = NewState;
            }
        }

        // TODO: Handle more than old states correctly
        [SerializeField] RelationshipState[] CurrentState;
        [SerializeField] RelationshipState NewState;
        [SerializeField] bool AllRelationships = false;
    }
}