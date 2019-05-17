using System;
using UnityEngine;

namespace cdv
{
    public sealed class ChangeRelationshipEffect : CardEffect
    {
        public override bool IsAsyncron => !AllRelationships;

        public override bool IsExecutionPossible(Player executer)
        {
            return ExistsMatchingRelationship(executer, CurrentState);
        }

        public override void Execute(Player owner)
        {
            if(!ExistsMatchingRelationship(owner, CurrentState))
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
                owner.RpcSetOldRelationships(CurrentState);
                owner.NewRelationshipState = NewState;
            }
            owner.PopulateRelationshipView = true;
        }

        bool ExistsMatchingRelationship(Player player, RelationshipState[] currentStates)
        {
            foreach(var p in player.GameManager.Players)
            {
                foreach(var relationship in p.Diplomacy.PlayerRelationships)
                {
                    if(Array.IndexOf(CurrentState, relationship.State) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [SerializeField] RelationshipState[] CurrentState;
        [SerializeField] RelationshipState NewState;
        [SerializeField] bool AllRelationships = false;
    }
}