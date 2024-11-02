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
        foreach (AnimalAspect currentAnimal in SystemAPI.Query<AnimalAspect>())
        {
            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();



            // if no existing target, scan
            if (!currentAnimal.IsTargetExist())
            {

                // check if Sensor CD is finished
                if (!currentAnimal.SensorIsReady(SystemAPI.Time.DeltaTime))
                {
                    continue; // if not finish, go to next animal's loop
                }
                //Debug.Log("Search Target: " + animal.entity.Index);

                
                int sensorNumber = currentAnimal.GetRandomSensorNumber(); // select random sensor to use
                //Debug.Log("sensorNumber: "+sensorNumber);

                currentAnimal.SetCurrentSensor(TargetCollisionLayers.targetLayers[sensorNumber]);

                // stored in static class TargetCollisionLayers
                //CollisionLayer[] targetLayers = { CollisionLayer.Grass, CollisionLayer.Animal }; // collide with corresponding layer acoording to sensorNumber



                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);


                physicsWorld.SphereCastAll(currentAnimal._localTransform.ValueRO.Position, currentAnimal.GetSensorSize() / 2,
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
                        if(hit.Entity == currentAnimal.entity) // hit itself
                            continue;

                        if (!SystemAPI.HasComponent<AnimalTag>(hit.Entity))
                        {
                            Debug.Log("Missing AnimalTag Component");
                            continue;
                        }

                        // compare size
                        if(currentAnimal.CompareTargetProperiesToHunt(SystemAPI.GetAspect<AnimalAspect>(hit.Entity)))
                        {
                            Debug.Log("Smaller, give up");
                            continue; // smaller than this hitted animal
                        }
                    }






                    // if eatable, set as target
                    Debug.Log("Target Found   " + currentAnimal.entity.Index);
                    currentAnimal.SetTargetEntity(hit.Entity);
                    currentAnimal.ResetChaseCountdown();
                    currentAnimal.targetPosition = SystemAPI.GetComponent<LocalTransform>(hit.Entity).Position;
                    

                    //Debug.Log(animal.targetPosition + "" + hit.Entity.Index+ ""+ SystemAPI.HasComponent<GrassProperties>(hit.Entity));
                    break;
                }


                // check if any Target locked after scanning
                if (!currentAnimal.IsTargetExist())
                {
                    // if failed to lock any target, mark this round as FAIL
                    currentAnimal.AdjustSensorProbability(false);
                }


                hits.Dispose();
            }
            // target exist
            else
            {
                // check status of locked target 
                Entity targetEntity = currentAnimal.GetTargetEntity();

                
                // check if target entity was destroyed
                if (!SystemAPI.Exists(targetEntity))
                {
                    // if destroyed, set target to null
                    Debug.Log("Target Entity Destroyed (no longer exists)");
                    currentAnimal.ClearTarget(false);
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
                            Debug.Log("Target lost   " + currentAnimal.entity.Index);

                            //reset target
                            currentAnimal.ClearTarget(false);

                            continue;
                        }
                    }
                    // target is an animal
                    else if (SystemAPI.HasComponent<AnimalTag>(targetEntity))
                    {

                        AnimalAspect targetAnimal = SystemAPI.GetAspect<AnimalAspect>(targetEntity);


                        // update target's threat entity & position
                        targetAnimal.SetThreatEntity(currentAnimal.entity, currentAnimal.position);
                        

                        // compare size
                        if (currentAnimal.CompareTargetProperiesToHunt(targetAnimal))
                        {
                            Debug.Log("Smaller, give up and reset target");

                            currentAnimal.ClearTarget(false); // clear target

                            targetAnimal.ClearThreat(currentAnimal.entity); // clear target threat

                            continue; // smaller than this hitted animal
                        }


                        // check remaining chase time
                        if (currentAnimal.IsChaseTimeOver(SystemAPI.Time.DeltaTime))
                        {
                            Debug.Log("Chase time over");

                            currentAnimal.ClearTarget(false); // clear target

                            targetAnimal.ClearThreat(currentAnimal.entity); // clear target threat

                            continue; 
                        }
                    }





                    // refresh target position
                    currentAnimal.targetPosition = SystemAPI.GetComponentRO<LocalTransform>(targetEntity).ValueRO.Position;
                    //Debug.Log(animal.targetPosition);

                    //Debug.Log("Distance: " + MathHelpers.GetDistance(animal._localTransform.ValueRO.Position, animal.targetPosition));

                }
                catch (ArgumentException e)
                {
                    Debug.Log("ArgumentException: Target Entity Destroyed (Sensor Trigger System)");
                    currentAnimal.ClearTarget(false);
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
