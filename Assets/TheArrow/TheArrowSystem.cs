using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using TurnBasedTutorial.Players;


namespace TurnBasedTutorial.HighlightArrow
{
    struct HasArrow : ISystemStateComponentData
    { }

    struct Arrow : IComponentData
    {
        public float3 animStart;
        public float bounceHeight;
        public float animSpeed;
    }

    public class TheArrowSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        EntityQuery _hasArrowQuery;

        Entity _arrowEntity;

        float _animSpeed;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnStartRunning()
        {
            // Disabled entities are ignored by queries unless you explicitly include it in the query
            _arrowEntity = GetEntityQuery(typeof(Arrow), typeof(Disabled)).GetSingletonEntity();

            // Disabled entities will automatically be ignored by queries
            EntityManager.AddComponent<Disabled>(_arrowEntity);
            _animSpeed = EntityManager.GetComponentData<Arrow>(_arrowEntity).animSpeed;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

            var arrowEntity = _arrowEntity;
            var arrow = EntityManager.GetComponentData<Arrow>(arrowEntity);

            // Move the arrow to the player with that's gotten an action. If multiple
            // players have an action at once this will not handle that properly
            inputDeps = Entities
                .WithAll<Player>()
                .WithAll<TurnAction>()
                .WithNone<HasArrow>()
                .ForEach((int entityInQueryIndex, Entity e, in Translation t) =>
                {
                // Re-enable the arrow
                buffer.RemoveComponent<Disabled>(entityInQueryIndex, arrowEntity);

                // Tag our acting player so it's excluded from future queries
                buffer.AddComponent<HasArrow>(entityInQueryIndex, e);

                // Update the arrow position and animation values
                buffer.SetComponent(entityInQueryIndex, arrowEntity, t);
                    arrow.animStart = t.Value;
                    buffer.SetComponent(entityInQueryIndex, arrowEntity, arrow);
                }).Schedule(inputDeps);


            float time = (float)Time.ElapsedTime * _animSpeed;

            // Animate the arrow.
            inputDeps = Entities
                .ForEach((ref Translation pos, in Arrow a) =>
                {
                    float3 p = pos.Value;

                    var t = Mathf.PingPong(time, 1);
                    p = math.lerp(a.animStart, a.animStart + new float3(0, a.bounceHeight, 0), t);

                    pos.Value = p;
                }).Schedule(inputDeps);

            // Hide the arrow when the player no longer has an action
            inputDeps = Entities
                .WithAll<Player>()
                .WithAll<HasArrow>()
                .WithNone<TurnAction>()
                .ForEach((int entityInQueryIndex, Entity e, in Translation t) =>
                {
                    buffer.AddComponent<Disabled>(entityInQueryIndex, arrowEntity);
                    buffer.RemoveComponent<HasArrow>(entityInQueryIndex, e);
                }).Schedule(inputDeps);

            _barrier.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
    } 
}
