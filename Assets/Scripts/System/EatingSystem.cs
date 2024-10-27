using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections;
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
        //state.Enabled = false;


        foreach(var(target, physicsCollider, transform, entity) in 
            SystemAPI.Query<RefRO<Target>, RefRO<PhysicsCollider>, RefRO<LocalTransform>>().WithEntityAccess())
        {

            if(target.ValueRO.targetEntity == Entity.Null)
            {
                // current animal no locked target, check next animal
                continue;
            }




            CollisionLayer collideWith = CollisionLayer.Grass;
            if (SystemAPI.HasComponent<AnimalTag>(target.ValueRO.targetEntity))
            {
                collideWith = CollisionLayer.Animal;
            }


            float3 entityPosition = transform.ValueRO.Position;

            // change filter Collision Layer by checking target type
            // check collision
            Entity collidedEntity = ColliderCast(entityPosition, physicsCollider.ValueRO, target.ValueRO.targetEntity, collideWith);



            // if not collided with any entity, check next animal
            if(collidedEntity == Entity.Null)
                continue;

            //Debug.Log(collidedEntity.Index == target.ValueRO.targetEntity.Index);


            // current animal collided with locked entity   // (Checked in function)

            /*
             if(collidedEntity != target.ValueRO.targetEntity)  
                continue;
            */


            // if collided with grass
            if (SystemAPI.HasComponent<GrassProperties>(collidedEntity))
            {
                GrassAspect grass = SystemAPI.GetAspect<GrassAspect>(collidedEntity);

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
                animal.ClearTarget(true);
            }
            // if collided with animal
            else if (SystemAPI.HasComponent<AnimalTag>(target.ValueRO.targetEntity))
            {
                AnimalAspect targetAnimal = SystemAPI.GetAspect<AnimalAspect>(collidedEntity);

                // check if target entity was destroyed
                if (!SystemAPI.Exists(collidedEntity))
                {
                    // if destroyed, set target to null
                    Debug.Log("Target Entity Destroyed (no longer exists)");
                    continue;
                }

                AnimalAspect animal = SystemAPI.GetAspect<AnimalAspect>(entity);



                float obtainedEnergy = targetAnimal.currentSize * SystemAPI.GetComponent<Cell>(collidedEntity).numberOfCell * 0.0005f;




                // eat the target animal
                animal.EatTarget(obtainedEnergy);


                // clear locked target
                animal.ClearTarget(true);

                // clear target threat
                targetAnimal.ClearTargetThreat(animal.entity);


                // set its energy to 0
                targetAnimal.WasEaten();

            }


        }
    }


    public unsafe Entity ColliderCast(float3 entityPosition, PhysicsCollider physicsCollider, Entity targetEntity, CollisionLayer targetLayer)
    {
        //Debug.Log("Check Collision");


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
            CollidesWith = (uint)targetLayer,
            GroupIndex = 0
        });




        ColliderCastInput input = new ColliderCastInput()
        {
            Collider = (Collider*)physicsColliderBlob.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = entityPosition,   // = RayFrom,
            End = entityPosition      // = RayTo

        };


        //ColliderCastHit hit = new ColliderCastHit();
        //bool haveHit = collisionWorld.CastCollider(input, out hit);

        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
        bool haveHit = collisionWorld.CastCollider(input,ref hits);


        // reset filter
        physicsColliderBlob.Value.SetCollisionFilter(new CollisionFilter
        {
            BelongsTo = (uint)CollisionLayer.Animal,
            CollidesWith = ~0u, // all 1s, so all layers, collide with everything
            GroupIndex = 0
        });




        Entity hittedTargetEntity = Entity.Null; // to be Return

        // check if collided with target entity
        if (haveHit)
        {
            
            foreach(ColliderCastHit hit in hits)
            {
                // Debug.Log("Collision ID: "+hit.Entity.Index);
                if(hit.Entity == targetEntity) // collided with target entity
                {
                    hittedTargetEntity = hit.Entity;
                    break;
                }
            }
            
        }
        
        hits.Dispose();


        
        //physicsColliderBlob.Dispose();

        return hittedTargetEntity; // if not hitted, return Entity.Null;
    }

}
