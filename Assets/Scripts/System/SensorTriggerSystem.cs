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

        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();



        //foreach ((RefRO<LocalTransform> localTransform, RefRO<AnimalSensor> trigger, Entity entity) in SystemAPI.Query< RefRO <LocalTransform>, RefRO <AnimalSensor>>().WithEntityAccess())
        foreach (AnimalAspect currentAnimal in SystemAPI.Query<AnimalAspect>())
        {
            //Debug.Log(currentAnimal.entity.Index + " is going to start sensor loop");




            // is it is chasing by another
            if (currentAnimal.IsThreatExist())
            {
                continue; // escape first, no need to scan food
            }





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




                // Store MIN distance entity
                float minDistance = -1; // set init value to negative number
                Entity nearestTargetEntity = Entity.Null;


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
                        if(!currentAnimal.IsAbleToHuntTarget(SystemAPI.GetAspect<AnimalAspect>(hit.Entity)))
                        {
                            Debug.Log(currentAnimal.entity.Index + " not able to hunt target, give up");
                            continue; // smaller than this hitted animal
                        }
                    }



                    float3 targetPosition = SystemAPI.GetComponentRO<LocalTransform>(hit.Entity).ValueRO.Position;
                    float targetDistance = MathHelpers.GetDistance(currentAnimal.position, targetPosition);


                    // if no problem, compare distance

                    if (targetDistance < minDistance || minDistance < 0) // new entity is more near OR mindistance is still storing init value
                    {
                        // update min distance
                        minDistance = targetDistance;
                        nearestTargetEntity = hit.Entity;
                        //Debug.Log(currentAnimal.entity.Index + " update min distance: " + minDistance);
                    }



                    /* Do Checking outside
                    // if eatable, set as target
                    Debug.Log("Target Found   " + currentAnimal.entity.Index);
                    currentAnimal.SetTargetEntity(hit.Entity);
                    currentAnimal.ResetChaseCountdown();
                    currentAnimal.targetPosition = SystemAPI.GetComponent<LocalTransform>(hit.Entity).Position;


                    //Debug.Log(animal.targetPosition + "" + hit.Entity.Index+ ""+ SystemAPI.HasComponent<GrassProperties>(hit.Entity));
                    break;
                    */
                }


                // if scanned any entity
                if (nearestTargetEntity != Entity.Null)
                {
                    // if eatable, set as target
                    //Debug.Log(currentAnimal.entity.Index + " found a target");
                    currentAnimal.SetTargetEntity(nearestTargetEntity);
                    currentAnimal.ResetChaseCountdown();
                    currentAnimal.targetPosition = SystemAPI.GetComponentRO<LocalTransform>(nearestTargetEntity).ValueRO.Position;   
                }
                /*
                else
                {
                    Debug.Log(currentAnimal.entity.Index + " cannot find any target");
                }
                */




                // check if any Target locked after scanning
                if (!currentAnimal.IsTargetExist())
                {
                    // if failed to lock any target, mark this round as FAIL
                    //Debug.Log(currentAnimal.entity.Index + " Adjust Sensor Probability checkpt 1");
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
                            Debug.Log(currentAnimal.entity.Index + " Grass Target lost");

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
                        //Debug.Log(currentAnimal.entity.Index + " Compare Size");
                        if (currentAnimal.IsAbleToHuntTarget(targetAnimal))
                        {
                            Debug.Log("Smaller, give up and reset target");

                            currentAnimal.ClearTarget(false); // clear target

                            targetAnimal.ClearThreat(currentAnimal.entity); // clear target threat

                            continue; // smaller than this hitted animal
                        }


                        // check remaining chase time
                        //Debug.Log(currentAnimal.entity.Index + " Check Chase Time: " + currentAnimal.remainChaseTime.ToString("0.000"));
                        if (currentAnimal.IsChaseTimeOver(SystemAPI.Time.DeltaTime))
                        {
                            Debug.Log(currentAnimal.entity.Index + " Chase time over");

                            currentAnimal.ClearTarget(false); // clear target

                            targetAnimal.ClearThreat(currentAnimal.entity); // clear target threat

                            continue; 
                        }
                        //Debug.Log(currentAnimal.entity.Index + " End of Checking Chase Time: " + currentAnimal.remainChaseTime.ToString("0.000"));
                    }





                    // refresh target position
                    currentAnimal.targetPosition = SystemAPI.GetComponentRO<LocalTransform>(targetEntity).ValueRO.Position;
                    //Debug.Log(animal.targetPosition);

                    //Debug.Log("Distance: " + MathHelpers.GetDistance(animal._localTransform.ValueRO.Position, animal.targetPosition));

                    //Debug.Log(currentAnimal.entity.Index + " End of update sensed target");

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
