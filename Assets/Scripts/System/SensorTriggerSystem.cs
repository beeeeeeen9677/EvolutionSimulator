using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(ConfigUpdateSystem))]
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

        // for handling in batches

        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator); // for disabling tag


        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();



        //foreach ((RefRO<LocalTransform> localTransform, RefRO<AnimalSensor> trigger, Entity entity) in SystemAPI.Query< RefRO <LocalTransform>, RefRO <AnimalSensor>>().WithEntityAccess())


        foreach (AnimalAspect currentAnimal in SystemAPI.Query<AnimalAspect>().WithAll<SensorTriggerTag>())
        {

            //Debug.Log(currentAnimal.entity.Index + " is going to start sensor loop");
            ecb.SetComponentEnabled<SensorTriggerTag>(currentAnimal.entity, false);


            // Existing logic here (for batch)



            // is it is chasing by another
            if (currentAnimal.IsThreatExist())
            {
                continue; // escape first, no need to scan food
            }




            if (currentAnimal.IsCurious())
                continue;



            // if no existing target, scan
            if (!currentAnimal.IsTargetExist())
            {

                // if this animal is full
                if (currentAnimal.hungerState == HungerStatusEnum.full)
                {
                    continue;
                }



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
                    BelongsTo = (uint)CollisionLayer.Sensor,
                    CollidesWith = (uint)TargetCollisionLayers.targetLayers[sensorNumber],    // (uint)CollisionLayer.Grass,
                });






                /*
                // Store MIN distance entity
                float minDistance = -1; // set init value to negative number
                Entity nearestTargetEntity = Entity.Null;
                */


                // Changed to List for sequencing
                // Store MIN distance entity
                List<float> minDistance = new List<float>(); // set init value to negative number
                List<Entity> nearestTargetEntity = new List<Entity>();


                int sequenceListLength = 1; // inital length for animal
                List<ColorCell> colorSequence = new List<ColorCell>();
                if (sensorNumber == 0) // grass
                {
                    // get the sequence of color
                    colorSequence = currentAnimal.GetGrassColorSequence();
                    sequenceListLength = colorSequence.Count; // change the length to 4
                }

                // init the Lists
                for (int i = 0; i < sequenceListLength; i++)
                {
                    minDistance.Add(-1); // set init value to negative number
                    nearestTargetEntity.Add(Entity.Null);
                }




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
                        if (hit.Entity == currentAnimal.entity) // hit itself
                            continue;

                        if (!SystemAPI.HasComponent<AnimalTag>(hit.Entity)) // do not has AnimalTag
                        {
                            Debug.Log("Missing AnimalTag Component");
                            continue;
                        }

                        // compare size
                        if (!currentAnimal.IsAbleToHuntTarget(SystemAPI.GetAspect<AnimalAspect>(hit.Entity)))
                        {
                            Debug.Log(currentAnimal.entity.Index + " not able to hunt target, give up");
                            continue; // smaller than this hitted animal
                        }


                        // check if target is family
                        if (currentAnimal.IsFamily(SystemAPI.GetAspect<AnimalAspect>(hit.Entity)))
                        {
                            Debug.Log(currentAnimal.entity.Index + " Target is family");
                            continue; // current target is family
                        }



                        // check if target in habitat
                        AnimalAspect targetAnimal = SystemAPI.GetAspect<AnimalAspect>(hit.Entity);
                        if (targetAnimal.IsInsideHabitat())
                        {
                            // get random number (0 - 1) and check with effect of hiding 
                            float hideEffectRandomNumber = UnityEngine.Random.Range(0, 1);
                            if (hideEffectRandomNumber < targetAnimal.habitatProperty.Value.effectOfHiding)
                            {
                                Debug.Log("Success to hide from predator"); // Success
                                continue;
                            }
                            Debug.Log("Failed to hide from predator"); // Failed


                        }



                    }



                    float3 targetPosition = SystemAPI.GetComponentRO<LocalTransform>(hit.Entity).ValueRO.Position;
                    float targetDistance = MathHelpers.GetDistance(currentAnimal.position, targetPosition);


                    // if no problem, compare distance
                    if (sensorNumber == 1) // check animal
                    {
                        int listIndex = 0;
                        UpdateNearestTargetList(minDistance, nearestTargetEntity, hit, targetDistance, listIndex);
                    }
                    else if (sensorNumber == 0) // check grass
                    {
                        int listIndex = -1;

                        // loop through different colors
                        for (int i = 0; i < colorSequence.Count; i++)
                        {
                            // Get the type of the current color for checking later
                            //Type typeOfCurrentColor = colorSequence[i].GetType();
                            // Convert the type to a ComponentType
                            //ComponentType componentType = ComponentType.ReadOnly(typeOfCurrentColor);

                            Type typeOfCurrentColor;
                            switch (colorSequence[i])
                            {
                                case ColorCell.Green:
                                    typeOfCurrentColor = typeof(GrassTag_Green);
                                    break;
                                case ColorCell.Orange:
                                    typeOfCurrentColor = typeof(GrassTag_Orange);
                                    break;
                                case ColorCell.Purple:
                                    typeOfCurrentColor = typeof(GrassTag_Purple);
                                    break;
                                case ColorCell.Pink:
                                    typeOfCurrentColor = typeof(GrassTag_Pink);
                                    break;

                                default:
                                    typeOfCurrentColor = typeof(AnimalTag); // this entity is not grass
                                    Debug.Log("Error: switch - grass color not matching");
                                    break;
                            }



                            if (state.EntityManager.HasComponent(hit.Entity, typeOfCurrentColor)) // if this grass has same color with current looping color
                            {
                                listIndex = i;
                                break;
                            }
                        }
                        if (listIndex == -1) // error
                        {
                            Debug.Log("Error: listIndex == -1 - grass color not matching");
                            listIndex = 0;
                        }


                        //Debug.Log(listIndex);
                        /*
                        // get the sequence of color
                        List<ColorCell> colorSequence = currentAnimal.GetGrassColorSequence();

                        foreach(ColorCell color in colorSequence)
                        {
                            // grt the type of current color for checking later
                            Type typeOfCurrentColor = color.GetType();

                        
                            if (SystemAPI.HasComponent<typeOfCurrentColor>(hit.Entity)) // if this grass have same color with current looping color
                            {

                            }
                        }
                        */
                        UpdateNearestTargetList(minDistance, nearestTargetEntity, hit, targetDistance, listIndex);
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


                for (int i = 0; i < nearestTargetEntity.Count; i++)
                {
                    // if scanned any entity
                    if (nearestTargetEntity[i] != Entity.Null)
                    {
                        // if eatable, set as target
                        //Debug.Log(currentAnimal.entity.Index + " found a target");
                        currentAnimal.SetTargetEntity(nearestTargetEntity[i]);
                        currentAnimal.ResetChaseCountdown();
                        currentAnimal.targetPosition = SystemAPI.GetComponentRO<LocalTransform>(nearestTargetEntity[i]).ValueRO.Position;

                        break;
                    }
                    /*
                    else
                    {
                        Debug.Log(currentAnimal.entity.Index + " cannot find any target");
                    }
                    */
                }





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
                        if (!currentAnimal.IsAbleToHuntTarget(targetAnimal))
                        {
                            Debug.Log(currentAnimal.entity.Index + ": Smaller, give up and reset target");

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


        ecb.Playback(state.EntityManager);//execute the recorded commands (disable tag)
        ecb.Dispose();

    }

    private static void UpdateNearestTargetList(List<float> minDistance, List<Entity> nearestTargetEntity, ColliderCastHit hit, float targetDistance, int listIndex)
    {
        /*
         * minDistance: List of mindistance to current nearest target
         * nearestTargetEntity: current nearest target
         * hit: current hitted(scanned) entity 
        */


        if (targetDistance < minDistance[listIndex] || minDistance[listIndex] < 0) // new entity is more near OR mindistance is still storing init value
        {
            // update min distance
            minDistance[listIndex] = targetDistance;
            nearestTargetEntity[listIndex] = hit.Entity;
            //Debug.Log(currentAnimal.entity.Index + " update min distance: " + minDistance);
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
