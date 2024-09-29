using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

public partial struct TestCollisionSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SphereComponent>();

        
        /*
        EntityManager entityManager = state.EntityManager;

        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);

        foreach (Entity entity in entities)
        {
            //if this entity has specific component
            if (entityManager.HasComponent<SphereComponent>(entity))
            {
                SphereComponent sphereComponent = entityManager.GetComponentData<SphereComponent>(entity);
                float3 entityPosition = SystemAPI.GetComponentRO<LocalTransform>(entity).ValueRO.Position;
                var physicsCollider = SystemAPI.GetComponent<PhysicsCollider>(entity);


                //BlobAssetReference<Collider> colliderBlob = physicsCollider.Value;



                var filter = new CollisionFilter()
                {
                    BelongsTo = (uint)CollisionLayer.Player,
                    CollidesWith = (uint)CollisionLayer.TestCollectable,
                    GroupIndex = 0
                };

                physicsCollider.Value.Value.SetCollisionFilter(filter);

                //colliderBlob.Dispose();

            }
        }

        entities.Dispose();
        */
        
    }


    private void OnUpdate(ref SystemState state)
    {
        //Debug.Log("Update");


        EntityManager entityManager = state.EntityManager;


        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);

        foreach (Entity entity in entities)
        {
            //if this entity has specific component
            if (entityManager.HasComponent<SphereComponent>(entity))
            {
                SphereComponent sphereComponent = entityManager.GetComponentData<SphereComponent>(entity);
                float3 entityPosition = SystemAPI.GetComponentRO<LocalTransform>(entity).ValueRO.Position;
                var physicsCollider = SystemAPI.GetComponent<PhysicsCollider>(entity);


                //BlobAssetReference<Collider> colliderBlob = physicsCollider.Value;


                //Entity collidedEntity = ColliderCast(entityPosition, entityPosition, physicsCollider);
                Entity collidedEntity = ColliderCast(entityPosition, entityPosition, 0.5f);


                //colliderBlob.Dispose();


                if (collidedEntity != Entity.Null)
                {
                    bool isSphere = SystemAPI.HasComponent<TestPhySphere>(collidedEntity);

                    if (!isSphere)
                        return;

                    Debug.Log(isSphere?collidedEntity.Index : "Not sphere");
                }

            }
        }

        entities.Dispose();
    }

    public unsafe Entity ColliderCast(float3 RayFrom, float3 RayTo, PhysicsCollider physicsCollider)
    {
        // Set up Entity Query to get PhysicsWorldSingleton
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        //EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
        /*
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        singletonQuery.Dispose();
        */


        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        /*
        var filter = new CollisionFilter()
        {
            BelongsTo = ~0u,
            CollidesWith = ~0u, // all 1s, so all layers, collide with everything
            GroupIndex = 0
        };

        SphereGeometry sphereGeometry = new SphereGeometry() { Center = float3.zero, Radius = radius };
        BlobAssetReference<Collider> sphereCollider = SphereCollider.Create(sphereGeometry, filter);
        */



        /*
        // copy the physics collider value
        PhysicsCollider newPhysicsCollider = new PhysicsCollider
        {
            Value = physicsCollider.Value
        };
        */



        BlobAssetReference<Collider> physicsColliderBlob = physicsCollider.Value;

        // change filter
        physicsColliderBlob.Value.SetCollisionFilter(new CollisionFilter
        {
            BelongsTo = (uint)CollisionLayer.Player,
            CollidesWith = (uint)CollisionLayer.TestCollectable,
            GroupIndex = 0
        });

    


        ColliderCastInput input = new ColliderCastInput()
        {
            Collider = (Collider*)physicsColliderBlob.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = RayFrom,
            End = RayTo
        };

        ColliderCastHit hit = new ColliderCastHit();
        bool haveHit = collisionWorld.CastCollider(input, out hit);


        // reset filter
        physicsColliderBlob.Value.SetCollisionFilter(new CollisionFilter
        {
            BelongsTo = (uint)CollisionLayer.Player,
            CollidesWith = ~0u, // all 1s, so all layers, collide with everything
            GroupIndex = 0
        });




        if (haveHit)
        {
            return hit.Entity;
        }

        //physicsColliderBlob.Dispose();

        return Entity.Null;
    }

    // overload
    public unsafe Entity ColliderCast(float3 RayFrom, float3 RayTo, float radius)
    {
        // Set up Entity Query to get PhysicsWorldSingleton
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        //EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
        /*
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        singletonQuery.Dispose();
        */


        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        
        var filter = new CollisionFilter()
        {
            BelongsTo = (uint)CollisionLayer.Player,
            CollidesWith = (uint)CollisionLayer.TestCollectable,
            GroupIndex = 0
        };

        SphereGeometry sphereGeometry = new SphereGeometry() { Center = float3.zero, Radius = radius };
        BlobAssetReference<Collider> sphereCollider = SphereCollider.Create(sphereGeometry, filter);
        

        ColliderCastInput input = new ColliderCastInput()
        {
            Collider = (Collider*)sphereCollider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = RayFrom,
            End = RayTo
        };

        ColliderCastHit hit = new ColliderCastHit();
        bool haveHit = collisionWorld.CastCollider(input, out hit);
        if (haveHit)
        {
            return hit.Entity;
        }

        sphereCollider.Dispose();

        return Entity.Null;
    }
}
