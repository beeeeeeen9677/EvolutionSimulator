using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct TestTriggerSystem : ISystem
{
    private void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;


        // Method 1 -------------------

        NativeArray<Entity> entites = entityManager.GetAllEntities(Allocator.Temp);

        foreach(Entity entity in entites)
        {
            if (entityManager.HasComponent<TestTriggerComponent>(entity))
            {
                RefRO<TestTriggerComponent> trigger = SystemAPI.GetComponentRO<TestTriggerComponent>(entity);

                // for TransformUsageFlags.None 
                RefRW<LocalToWorld> triggerTransform = SystemAPI.GetComponentRW<LocalToWorld>(entity);

                float size = trigger.ValueRO.size;
                triggerTransform.ValueRW.Value.c0 = new Unity.Mathematics.float4(size, 0, 0, 0);
                triggerTransform.ValueRW.Value.c1 = new Unity.Mathematics.float4(0, size, 0, 0);
                triggerTransform.ValueRW.Value.c2 = new Unity.Mathematics.float4(0, 0, size, 0);


                PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                //physicsWorld.SphereCastAll(triggerTransform.ValueRO.Position, size / 2,
                //    float3.zero, 1, ref hits, CollisionFilter.Default);
                physicsWorld.SphereCastAll(triggerTransform.ValueRO.Position, size / 2,
                    float3.zero, 1, ref hits, 
                    new CollisionFilter
                    {
                        BelongsTo = (uint)CollisionLayer.Player,
                        CollidesWith = (uint)CollisionLayer.TestCollectable,
                    });

                foreach (ColliderCastHit hit in hits)
                {
                    //entityManager.DestroyEntity(hit.Entity);
                    Debug.Log("Trigger: " + hit.Entity);
                }

                hits.Dispose();
            }
        }


        // Method 2 -------------------
        /*
         *        
        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach ((RefRO<TriggerComponent> trigger, Entity entity) in SystemAPI.Query<RefRO<TriggerComponent>>().WithEntityAccess())
        {
            // only works for TransformUsageFlags.Dynamic  // has LocalTransform
            //RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(entity);
            //localTransform.ValueRW.Scale = trigger.ValueRO.size;


            // for TransformUsageFlags.None 
            RefRW<LocalToWorld> triggerTransform = SystemAPI.GetComponentRW<LocalToWorld>(entity);

            float size = trigger.ValueRO.size;
            triggerTransform.ValueRW.Value.c0 = new Unity.Mathematics.float4(size, 0, 0, 0);
            triggerTransform.ValueRW.Value.c1 = new Unity.Mathematics.float4(0, size, 0, 0);
            triggerTransform.ValueRW.Value.c2 = new Unity.Mathematics.float4(0, 0, size, 0);


            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

            physicsWorld.SphereCastAll(triggerTransform.ValueRO.Position, size / 2,
                float3.zero, 1, ref hits, CollisionFilter.Default);


            foreach(ColliderCastHit hit in hits)
            {
                //entityManager.DestroyEntity(hit.Entity);
                ecb.DestroyEntity(hit.Entity);
                //Debug.Log(hit.Entity);
            }

        }

        ecb.Playback(entityManager);


        hits.Dispose();

        */
    }
}


