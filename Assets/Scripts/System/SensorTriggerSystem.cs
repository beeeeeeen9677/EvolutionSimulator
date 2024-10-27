using System;
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
        //state.Enabled = false; // for debug lagging



        //foreach ((RefRO<LocalTransform> localTransform, RefRO<AnimalSensor> trigger, Entity entity) in SystemAPI.Query< RefRO <LocalTransform>, RefRO <AnimalSensor>>().WithEntityAccess())
        foreach (AnimalAspect animal in SystemAPI.Query<AnimalAspect>())
        {
            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();



            // if no existing target, scan
            if (!animal.IsTargetExist())
            {

                // check if Sensor CD is finished
                if (!animal.SensorIsReady(SystemAPI.Time.DeltaTime))
                {
                    continue; // if not finish, go to next animal's loop
                }
                //Debug.Log("Search Target: " + animal.entity.Index);

                
                int sensorNumber = animal.GetRandomSensorNumber(); // select random sensor to use
                //Debug.Log("sensorNumber: "+sensorNumber);

                // stored in static class TargetCollisionLayers
                //CollisionLayer[] targetLayers = { CollisionLayer.Grass, CollisionLayer.Animal }; // collide with corresponding layer acoording to sensorNumber



                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);


                physicsWorld.SphereCastAll(animal._localTransform.ValueRO.Position, animal.GetSensorSize() / 2,
                float3.zero, 1, ref hits, new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.Animal,
                    CollidesWith = (uint)TargetCollisionLayers.targetLayers[sensorNumber],    // (uint)CollisionLayer.Grass,
                });

                // loop through all scaned entities
                foreach (ColliderCastHit hit in hits)
                {
                    //Debug.Log("Scaned Target: " + hit.Entity.Index);


                    // check grass
                    if (sensorNumber == 0)
                    {
                        if (!SystemAPI.HasComponent<GrassProperties>(hit.Entity))
                        {
                            Debug.Log("Missing GrassProperties Component");
                            continue;
                        }

                        var grass = SystemAPI.GetComponentRO<GrassProperties>(hit.Entity);

                        // this grass is not activated
                        if (!grass.ValueRO.activated)
                            continue; // check next detected entity


                    }
                    // check animal
                    else if (sensorNumber == 1)
                    {
                        if(hit.Entity == animal.entity) // hit itself
                            continue;

                        if (!SystemAPI.HasComponent<AnimalTag>(hit.Entity))
                        {
                            Debug.Log("Missing AnimalTag Component");
                            continue;
                        }

                        // compare size
                        if(animal.CompareTargetProperiesToHunt(SystemAPI.GetAspect<AnimalAspect>(hit.Entity)))
                        {
                            Debug.Log("Smaller, give up");
                            continue; // smaller than this hitted animal
                        }
                    }






                    // if eatable, set as target
                    Debug.Log("Target Found   " + animal.entity.Index);
                    animal.SetTargetEntity(hit.Entity);
                    animal.targetPosition = SystemAPI.GetComponent<LocalTransform>(hit.Entity).Position;
                    //Debug.Log(animal.targetPosition + "" + hit.Entity.Index+ ""+ SystemAPI.HasComponent<GrassProperties>(hit.Entity));
                    break;
                }

                hits.Dispose();
            }
            // target exist
            else
            {
                // check status of locked target 
                Entity targetEntity = animal.GetTargetEntity();

                
                // check if target entity was destroyed
                if (!SystemAPI.Exists(targetEntity))
                {
                    // if destroyed, set target to null
                    Debug.Log("Target Entity Destroyed (no longer exists)");
                    animal.ClearTarget();
                    continue;
                }


                // for handling ArgumentException target animal entity destroyed
                try
                {


                    // target is a grass
                    if (SystemAPI.HasComponent<GrassProperties>(targetEntity))
                    {
                        var grass = SystemAPI.GetComponentRO<GrassProperties>(targetEntity);

                        // if this grass is not activated now
                        if (!grass.ValueRO.activated)
                        {
                            Debug.Log("Target lost   " + animal.entity.Index);

                            //reset target
                            animal.ClearTarget();

                            continue;
                        }
                    }
                    // target is an animal
                    else if (SystemAPI.HasComponent<AnimalTag>(targetEntity))
                    {
                        // compare size
                        if (animal.CompareTargetProperiesToHunt(SystemAPI.GetAspect<AnimalAspect>(targetEntity)))
                        {
                            Debug.Log("Smaller, give up and reset target");
                            animal.ClearTarget();
                            continue; // smaller than this hitted animal
                        }
                    }





                    // refresh target position
                    animal.targetPosition = SystemAPI.GetComponentRO<LocalTransform>(targetEntity).ValueRO.Position;
                    //Debug.Log(animal.targetPosition);

                    //Debug.Log("Distance: " + MathHelpers.GetDistance(animal._localTransform.ValueRO.Position, animal.targetPosition));

                }
                catch (ArgumentException e)
                {
                    Debug.Log("ArgumentException: Target Entity Destroyed (Sensor Trigger System)");
                    animal.ClearTarget();
                    continue;
                }
            }
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
