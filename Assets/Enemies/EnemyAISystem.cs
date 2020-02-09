using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using TurnBasedTutorial.Movement;

namespace TurnBasedTutorial.Enemies
{
    public class EnemyAISystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        EntityQuery _actingAI;

        Random _random;

        protected override void OnCreate()
        {
            _random = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

            int actingAICount = _actingAI.CalculateEntityCount();

            NativeArray<int2> directions = new NativeArray<int2>(actingAICount, Allocator.TempJob);

            // Generate random directions
            for (int i = 0; i < actingAICount; ++i)
                directions[i] = GetRandomDirection();

            inputDeps = Entities
                .WithStoreEntityQueryInField(ref _actingAI)
                .WithAll<Enemy>()
                // Only act when we have a turn
                .WithAll<TurnAction>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    buffer.AddComponent<TryMove>(entityInQueryIndex, e, directions[entityInQueryIndex]);
                }).Schedule(inputDeps);

            _barrier.CreateCommandBuffer().RemoveComponent<TurnAction>(_actingAI);
            _barrier.AddJobHandleForProducer(inputDeps);

            directions.Dispose(inputDeps);

            return inputDeps;
        }

        int2 GetRandomDirection()
        {
            int r = _random.NextInt(0, 5);
            switch(r)
            {
                case 0: return new int2(1, 0);
                case 1: return new int2(-1, 0);
                case 3: return new int2(0, 1);
                case 4: return new int2(0, -1);
            }
            return 0;
        }

 
    } 
}
