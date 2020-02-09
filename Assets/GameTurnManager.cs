using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;

using TurnBasedTutorial.Players;
using TurnBasedTutorial.Enemies;
using System;

namespace TurnBasedTutorial
{



    /// <summary>
    /// <para>A job system to manage the "Game Turn" cycle. Inherit from <see cref="GameTurnSystem{ActorType}"/>
    /// to implement a system that can "take a turn".</para>
    /// </summary>
    [AlwaysUpdateSystem]
    public class GameTurnManager : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        int _currentTurnIndex;

        bool _turnIsActive = false;

        /// <summary>
        /// An entity used to manage game turns.
        /// </summary>
        Entity _gameTurnEntity;

        /// <summary>
        /// Tag types associated with any "Game Turn" added via <seealso cref="AddGameTurn{TurnType}"/>.
        /// </summary>
        List<ComponentType> _turnTypes = new List<ComponentType>();

        /// <summary>
        /// Queries for any "Game Turn" added via <seealso cref="AddGameTurn{TurnType}"/>. The queries
        /// are used to determine when a turn is complete.
        /// </summary>
        //List<EntityQuery> _activeTurnQueries = new List<EntityQuery>();

        /// <summary>
        /// Call this to add a "Game Turn" to the turn cycle. <typeparamref name="TurnType"/> refers to 
        /// tag component associated with this game turn. This component will be created by the 
        /// <see cref="GameTurnManager"/> when the turn starts and the next turn won't begin until 
        /// the tag component is destroyed by whatever system is "taking a turn".
        /// </summary>
        public void AddGameTurn<TurnType>() where TurnType : IComponentData
        {
            _turnTypes.Add(ComponentType.ReadOnly<TurnType>());
        }


        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            
            _gameTurnEntity = EntityManager.CreateEntity(
                typeof(GameTurn)
                );

#if UNITY_EDITOR
            EntityManager.SetName(_gameTurnEntity, "TurnManagerEntity");
#endif

            // Add our game turns
            AddGameTurn<PlayerTurn>();
            AddGameTurn<EnemyTurn>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_turnTypes.Count == 0)
                return inputDeps;
            
            if( !_turnIsActive )
            {
                _turnIsActive = true;

                BeginTurn();
                
                return inputDeps;
            }


            if( _turnIsActive && TurnIsComplete() )
            {
                _turnIsActive = false;
                _currentTurnIndex = (_currentTurnIndex + 1) % _turnTypes.Count;
            }

            return inputDeps;
        }

        void BeginTurn()
        {
            _barrier.CreateCommandBuffer()
                .AddComponent(_gameTurnEntity, _turnTypes[_currentTurnIndex] );
        }

        bool TurnIsComplete()
        {
            return !EntityManager.HasComponent(_gameTurnEntity, _turnTypes[_currentTurnIndex]);
        }
    } 
}
