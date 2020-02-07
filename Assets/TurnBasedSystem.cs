using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;

public struct TurnAction : IComponentData
{
}

[AlwaysUpdateSystem]
public class TurnBasedSystem : JobComponentSystem
{
    EntityQuery _actingPlayers;

    EntityQuery _actingAI;
    EntityQuery _nonActingAI;

    EndSimulationEntityCommandBufferSystem _barrier;

    int _currentlyActingPlayer = -1;
    int _playerCount = 0;
    
    bool _playersActing;

    InputActions _controls;
    InputAction _clearActions;

    protected override void OnCreate()
    {
        _actingPlayers = GetEntityQuery(
            ComponentType.ReadOnly<PlayerActor>(),
            ComponentType.ReadOnly<TurnAction>()
            );
        
        _actingAI = GetEntityQuery(
            ComponentType.ReadOnly<AIActor>(),
            ComponentType.ReadOnly<TurnAction>()
            );

        _nonActingAI = GetEntityQuery(
            ComponentType.ReadOnly<AIActor>(),
            ComponentType.Exclude<TurnAction>()
            );
        
        _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        _controls = new InputActions();
        _controls.Enable();

        _clearActions = _controls.Default.ClearActions;
    }

    protected override void OnStartRunning()
    {
        _playerCount = GetEntityQuery(typeof(PlayerActor)).CalculateEntityCount();

        Debug.Log($"Initializing turn based system with {_playerCount} players.");
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = _barrier.CreateCommandBuffer();

        int actingPlayerCount = _actingPlayers.CalculateEntityCount();
        int aiActingCount = _actingAI.CalculateEntityCount();

        if( _clearActions.triggered )
        {
            Debug.Log("Clearing actions");
            // Clear all actions
            commandBuffer.RemoveComponent<TurnAction>(EntityManager.UniversalQuery);
        }

        // If any AI are acting, we do nothing until they're finished.
        if ( aiActingCount > 0)
        {
            // Temp : For now clear actions
            commandBuffer.RemoveComponent<TurnAction>(_actingAI);
            _barrier.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
        

        // Initialize the "playersActing" phase
        if ( !_playersActing && actingPlayerCount == 0)
        {
            _playersActing = true;
            _currentlyActingPlayer = 0;
        }

        // Hand out an action to the next player as the currently acting player finishes until we've
        // let all players act.
        if( _playersActing && actingPlayerCount == 0 && _currentlyActingPlayer < _playerCount )
        {
            Debug.Log("Handing action to player " + _currentlyActingPlayer);

            var concurrentBuffer = commandBuffer.ToConcurrent();

            var curr = _currentlyActingPlayer++;
            
            // Hand out the action to the correct player
            inputDeps = Entities.ForEach((int entityInQueryIndex, Entity e, in Player p) =>
            {
                if (p.number == curr)
                    concurrentBuffer.AddComponent<TurnAction>(entityInQueryIndex, e);
            }).Schedule(inputDeps);

            _barrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
        
        // Once all players have acted we let the AI act
        if( _playersActing && actingPlayerCount == 0 )
        {
            _playersActing = false;
            Debug.Log("Handing out ai actions");
            commandBuffer.AddComponent<TurnAction>(_nonActingAI);
        }

        _barrier.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}
