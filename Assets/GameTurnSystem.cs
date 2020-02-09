using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TurnBasedTutorial
{
    
    /// <summary>
    /// Used by <see cref="GameTurnSystem{TurnType, ActorType}"/> to provide easy 
    /// queries for entities that have an action during a game turn.
    /// </summary>
    public struct TurnAction : IComponentData
    { }

    /// <summary>
    /// <para>Classes can implement this to represent a game turn.</para>
    /// 
    /// <para>"<typeparamref name="TurnType"/>" is a tag component used internally for managing turn order. It should be 
    /// unique and used only for this purpose. 
    /// IE: "PlayerTurn", "EnemyTurn", "AITurn", etc.</para>
    /// 
    /// <para>"<typeparamref name="ActorType"/>" refers
    /// to the component type used to represent the "actors" for this turn. 
    /// Only used to provide convenience queries for easier turn distribution via 
    /// <see cref="_actorsWithActions"/> and <see cref="_actorsWithoutActions"/>.
    /// IE: "Player", "Enemy", "AI", etc.
    /// </para>
    /// </summary>
    public abstract class GameTurnSystem<TurnType, ActorType> : JobComponentSystem 
        where TurnType : IComponentData
        where ActorType : IComponentData
    {
        /// <summary>
        /// A query for the actors in this turn that currently do not have a <see cref="TurnAction"/>.
        /// </summary>
        protected EntityQuery _actorsWithoutActions;

        /// <summary>
        /// A query for actors in this turn which currently have a <see cref="TurnAction"/>.
        /// </summary>
        protected EntityQuery _actorsWithActions;
        
        /// <summary>
        /// Gives a <see cref="TurnAction"/> to the entity.
        /// </summary>
        protected void AssignAction(Entity e)
        {
            _commandBuffer.AddComponent<TurnAction>(e);
        }

        /// <summary>
        /// Gives a <see cref="TurnAction"/> to a set of entities.
        /// </summary>
        protected void AssignAction(EntityQuery e)
        {
            _commandBuffer.AddComponent<TurnAction>(e);
        }

        /// <summary>
        /// Should be called manually once the system's turn is complete.
        /// </summary>
        protected void EndTurn()
        {
            // We signal the end of a turn by removing our turn tag from the "TurnManagerEntity".
            _commandBuffer.RemoveComponent<TurnType>(_turnManagerQuery);
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _turnManagerQuery = GetEntityQuery(
                 ComponentType.ReadOnly<GameTurn>(),
                 ComponentType.ReadOnly<TurnType>()
                 );

            _actorsWithoutActions = GetEntityQuery(
                ComponentType.ReadOnly<ActorType>(),
                ComponentType.Exclude<TurnAction>()
                );
            
            _actorsWithActions = GetEntityQuery(
                ComponentType.ReadOnly<ActorType>(),
                ComponentType.ReadOnly<TurnAction>()
                );

            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireForUpdate(_turnManagerQuery);
        }

        protected sealed override void OnStartRunning()
        {

            Debug.Log($"Beginning {typeof(ActorType).Name} turn.");

            _commandBuffer = _endSimBarrier.CreateCommandBuffer();

            OnTurnBegin();
        }

        protected sealed override void OnStopRunning()
        {
            Debug.Log($"Ending {typeof(TurnType).Name} turn.");

            OnTurnEnd();
        }
        
        protected sealed override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _commandBuffer = _endSimBarrier.CreateCommandBuffer();

            var deps = OnTurnUpdate(inputDeps);

            _endSimBarrier.AddJobHandleForProducer(deps);

            return deps;
        }
        
        protected abstract void OnTurnBegin();
        protected abstract JobHandle OnTurnUpdate(JobHandle inputDeps);
        protected virtual void OnTurnEnd() {}
        
        protected EndSimulationEntityCommandBufferSystem _endSimBarrier;
        protected EntityCommandBuffer _commandBuffer;

        EntityQuery _turnManagerQuery;
    }
}