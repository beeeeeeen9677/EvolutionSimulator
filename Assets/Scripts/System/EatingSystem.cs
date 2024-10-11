using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Collider = Unity.Physics.Collider;



// checking Collision
public partial struct EatingSystem : ISystem
{
    //private PhysicsWorldSingleton physicsWorldSingleton;


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Target>();
        //state.RequireForUpdate<PhysicsWorld>();
        //physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        foreach(var(target, physicsCollider, transform, entity) in 
            SystemAPI.Query<RefRO<Target>, RefRO<PhysicsCollider>, RefRO<LocalTransform>>().WithEntityAccess())
        {

            if(target.ValueRO.targetEntity == Entity.Null)
            {
                // current animal no locked target, check next animal
                continue;
            }





            float3 entityPosition = transform.ValueRO.Position;

            // change filter Collision Layer by checking target type
            // check collision
            Entity collidedEntity = ColliderCast(entityPosition, entityPosition, physicsCollider.ValueRO);




            // if not collided with any entity, check next animal
            if(collidedEntity == Entity.Null)
                continue;
            
            //Debug.Log(collidedEntity.Index == target.ValueRO.targetEntity.Index);

            // current animal collided with locked entity
            if(collidedEntity.Index != target.ValueRO.targetEntity.Index)
                continue;


            // if collided with grass
            if (SystemAPI.HasComponent<GrassProperties>(collidedEntity))
            {
                var grass = SystemAPI.GetAspect<GrassAspect>(collidedEntity);

                // if this grass is not activated (was eaten by others)
                if(grass.activated == false)
                    continue;



                float obtainedEnergy = grass.currentSize * grass.provideEnergy;

                AnimalAspect animal = SystemAPI.GetAspect<AnimalAspect>(entity);

                // eat the grass
                animal.EatTarget(obtainedEnergy);

                // set isActivated to false
                grass.WasEaten();

                // clear locked target
                animal.ClearTarget();
            }    
                
            
        }
    }


    public unsafe Entity ColliderCast(float3 RayFrom, float3 RayTo, PhysicsCollider physicsCollider)
    {
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        /*
        // Set up Entity Query to get PhysicsWorldSingleton

        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        singletonQuery.Dispose();
        */


        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        //CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

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
            BelongsTo = (uint)CollisionLayer.Animal,
            CollidesWith = (uint)CollisionLayer.Grass,
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
            BelongsTo = (uint)CollisionLayer.Animal,
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

}
