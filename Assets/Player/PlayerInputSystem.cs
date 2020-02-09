using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

using TurnBasedTutorial.Movement;

namespace TurnBasedTutorial.Players
{
    public class PlayerInputSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        InputActions _actions;

        InputAction _moveInput;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _actions = new InputActions();
            _actions.Enable();

            _moveInput = _actions.Default.Move;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

            float2 move = 0;
            if (_moveInput.triggered)
                move = _moveInput.ReadValue<Vector2>();

            inputDeps = Entities
                .WithAll<Player>()
                .WithAll<TurnAction>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    if (move.x != 0 || move.y != 0)
                    {
                        buffer.AddComponent<TryMove>(entityInQueryIndex, e, (int2)move);

                    // In this case we count all attempted moves as an action - even if it
                    // turns out to be an invalid move. If we wanted to change that behaviour
                    // we would check for a valid move in MoveSystem and remove the action then
                    buffer.RemoveComponent<TurnAction>(entityInQueryIndex, e);
                    }
                }).Schedule(inputDeps);

            _barrier.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
    } 
}
