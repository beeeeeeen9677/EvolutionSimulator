using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct SensorTriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimalSensor>();
    }


    public void OnUpdate(ref SystemState state)
    {
        /*
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);

        new ScanTargetJob
        {
            physicsWorld = physicsWorld,
            hits = hits,
        }.Schedule();


        hits.Dispose();
        */



        //foreach ((RefRO<LocalTransform> localTransform, RefRO<AnimalSensor> trigger, Entity entity) in SystemAPI.Query< RefRO <LocalTransform>, RefRO <AnimalSensor>>().WithEntityAccess())
        foreach (AnimalAspect animal in SystemAPI.Query<AnimalAspect>())
        {
            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);


            // if no existing target, scan
            if (!animal.IsTargetExist())
            {
                physicsWorld.SphereCastAll(animal._localTransform.ValueRO.Position, animal.GetSensorSize() / 2,
                float3.zero, 1, ref hits, new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.Animal,
                    CollidesWith = (uint)CollisionLayer.Grass,
                });

                // loop through all scaned entities
                foreach (ColliderCastHit hit in hits)
                {
                    //Debug.Log("Scaned Target: " + hit.Entity.Index);

                    if (!SystemAPI.HasComponent<GrassProperties>(hit.Entity))
                    {
                        Debug.Log("Missing GrassProperties Component");
                        break;
                    }

                    GrassAspect grass = SystemAPI.GetAspect<GrassAspect>(hit.Entity);


                    // not eatable
                    if (!grass.activated)
                        continue; // check next scaned entity



                    // if eatable, set as target
                    animal.SetTargetEntity(hit.Entity);
                    animal.targetPosition = SystemAPI.GetComponent<LocalTransform>(hit.Entity).Position;
                    //Debug.Log(animal.targetPosition + "" + hit.Entity.Index+ ""+ SystemAPI.HasComponent<GrassProperties>(hit.Entity));
                    break;
                }
            }
            else
            {
                // refresh target position
                animal.targetPosition = SystemAPI.GetComponentRO<LocalTransform>(animal.GetTargetEntity()).ValueRO.Position;
                Debug.Log(animal.targetPosition);

            }


            hits.Dispose();
        }
        
    }
}


/*
public partial struct ScanTargetJob : IJobEntity
{
    public PhysicsWorldSingleton physicsWorld;
    public NativeList<ColliderCastHit> hits;

    public void Execute(AnimalAspect animal)
    {
        // reset the list 
        hits.Clear();

        if (animal.TargetExist())
        {
            physicsWorld.SphereCastAll(animal._localTransform.ValueRO.Position, animal.GetSensorSize() / 2,
                float3.zero, 1, ref hits, new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.Animal,
                    CollidesWith = (uint)CollisionLayer.Grass,
                });

            foreach (ColliderCastHit hit in hits)
            {
                Debug.Log("Scaned Target: " + hit.Entity.Index);
            }
        }
    }
}
*/
