using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct TryMove : IComponentData
{
    public float2 dir;
}

public class MoveSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem _barrier;

    EntityQuery _moveQuery;

    EntityQuery _stageQuery;

    EntityQuery _collidableQuery;

    protected override void OnCreate()
    {
        _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        _collidableQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Collidable>()
            );
        _stageQuery = GetEntityQuery(
            ComponentType.ReadOnly<Stage>()
            );
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var collidables = _collidableQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        var stage = _stageQuery.GetSingleton<Stage>();

        inputDeps = Entities
            .WithStoreEntityQueryInField(ref _moveQuery)
            .ForEach((ref Translation t, in TryMove move) =>
            {
                int2 targetPos = (int2)math.round(t.Value.xz + move.dir);

                if (targetPos.x < 0 || targetPos.x >= stage.size.x ||
                    targetPos.y < 0 || targetPos.y >= stage.size.y)
                    return;

                for( int i = 0; i < collidables.Length; ++i )
                {
                    var cPos = (int2)collidables[i].Value.xz;
                    if (targetPos.x == cPos.x && targetPos.y == cPos.y)
                        return;
                }

                t.Value = new float3(targetPos.x, t.Value.y, targetPos.y);
            }).Schedule(inputDeps);
        
        _barrier.CreateCommandBuffer().RemoveComponent<TryMove>(_moveQuery);

        collidables.Dispose(inputDeps);

        return inputDeps;
    }
}
