
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TurnBasedTutorial.Players
{
    public struct PlayerTurn : IComponentData { }
    
    /// <summary>
    /// A game turn which hands actions out to players one at a time in order based on their player number.
    /// </summary>
    public class PlayerTurnSystem : GameTurnSystem<PlayerTurn, Player>
    {
        NativeArray<Entity> _players;

        int _currentPlayerIndex = 0;
        
        protected override void OnTurnBegin()
        {
            if(_actorsWithoutActions.CalculateEntityCount() == 0 )
            {
                Debug.LogWarning("Attempting to start player turn, but no players were found.");
                EndTurn();
                return;
            }

            _players = _actorsWithoutActions.ToEntityArray(Allocator.Persistent);
            
            // Sort players based on their player number
            _players.Sort(new PlayerCompare(EntityManager));

            Debug.Log($"Found {_players.Length} players");
        }

        protected override void OnTurnEnd()
        {
            _players.Dispose();
        }

        protected override JobHandle OnTurnUpdate(JobHandle inputDeps)
        {
            // Assign actions to the next player in order as the current player
            // finishes their action
            if(_actorsWithActions.CalculateEntityCount() == 0 )
            {
                if (_currentPlayerIndex < _players.Length)
                {
                    Debug.Log("Assigning action to player " + _currentPlayerIndex);
                    AssignAction(_players[_currentPlayerIndex++]);
                }
                else
                {
                    _currentPlayerIndex = 0;
                    EndTurn();
                }

                return inputDeps;
            }
            
            return inputDeps;
        }

        /// <summary>
        /// Sort players by their number.
        /// </summary>
        struct PlayerCompare : IComparer<Entity>
        {
            EntityManager _em;

            public PlayerCompare(EntityManager em)
            {
                _em = em;
            }

            public int Compare(Entity a, Entity b)
            {
                return _em.GetComponentData<Player>(a).number.CompareTo(
                    _em.GetComponentData<Player>(b).number);
            }
        }
    }
}