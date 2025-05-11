using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(ConfigUpdateSystem))]
public partial struct ReproductionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Energy>();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach ((GrowUpAspect animal, RefRO<Energy> energy) in SystemAPI.Query<GrowUpAspect, RefRO<Energy>>().WithAll<ReproductionTag>())
        {
            entityCommandBuffer.SetComponentEnabled<ReproductionTag>(animal.entity, false); // disable the tag after the first update


            // Check and Count Down do in Counter Handle System
            /*
            //if (animal.currentStage != AgeStageEnum.mature)
            if (animal.currentStage == AgeStageEnum.infant)  // only generate offspring if not at infant stage
               continue;

            // if current energy more than 50% of max energy, born offspring
            if(energy.ValueRO.currentEnergy >= 0.7f * energy.ValueRO.maxEnergy)
            {
                if (animal.IsReadyToGenerateOffspring(SystemAPI.Time.DeltaTime))
                {
                    GenerateOffspring(animal.entity, entityCommandBuffer, ref state);
                }
            }
            */


            GenerateOffspring(animal.entity, entityCommandBuffer, ref state);

        }


        entityCommandBuffer.Playback(state.EntityManager);//execute the recorded commands
        entityCommandBuffer.Dispose();
    }

    private void GenerateOffspring(Entity parentEntity, EntityCommandBuffer entityCommandBuffer, ref SystemState state)
    {
        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();



        AnimalAspect parentAnimal = SystemAPI.GetAspect<AnimalAspect>(parentEntity);

        parentAnimal.ConsumeFixedEnergy(initAnimalConfig.reproductionCost * parentAnimal.maxEnergy);        // Consume 20% energy



        int numberToSpawn = 1; // initial value

        // small animals generate more offspring
        float sizeRatio = (parentAnimal.currentSize - initAnimalConfig.minInitSize) / (initAnimalConfig.maxInitSize - initAnimalConfig.minInitSize);
        if(sizeRatio <= 0.4f)
        {
            numberToSpawn += 1;
        }



        // animals eat grass generate more offspring
        if (parentAnimal.grassSensorProbability >= 0.6f)
        {
            numberToSpawn += 1;
        }
      




        for (int i = 0; i < numberToSpawn; i++)
        {
            Debug.Log($"{parentEntity.Index} Born offspring: {i + 1} of {numberToSpawn}");


            // born new animal
            Entity newSpawnedAnimal = entityCommandBuffer.Instantiate(initAnimalConfig.animalPrefab);


            // SizeProperty Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, SystemAPI.GetComponent<SizeProperty>(parentEntity));


            // LocalTransform Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new LocalTransform
            {
                Position = new float3(
                    UnityEngine.Random.Range(parentAnimal.position.x - 3, parentAnimal.position.x + 3),  // x
                    parentAnimal.position.y + 1,                                                       // y
                    UnityEngine.Random.Range(parentAnimal.position.z - 3, parentAnimal.position.z + 3)), // z
                Rotation = quaternion.identity,
                Scale = SystemAPI.GetComponent<SizeProperty>(parentEntity).initSize
            });


            // Movement Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, SystemAPI.GetComponent<Movement>(parentEntity));


            // Energy Component
            var parentEnergy = SystemAPI.GetComponent<Energy>(parentEntity);


            entityCommandBuffer.SetComponent(newSpawnedAnimal, new Energy
            {
                maxEnergy = parentEnergy.maxEnergy,
                currentEnergy = parentAnimal.maxEnergy,
            });


            // AnimalSensor Component
            AnimalSensor parentSensor = SystemAPI.GetComponent<AnimalSensor>(parentEntity);




            // store as variable, will be used to set the color
            float newGrassSensorProb = parentSensor.grassSensorProbability;
            float newAnimalSensorProb = 1 - parentSensor.grassSensorProbability;



            entityCommandBuffer.SetComponent(newSpawnedAnimal, new AnimalSensor
            {
                size = parentSensor.size,

                warningRange = parentSensor.warningRange,

                maxCooldown = parentSensor.maxCooldown,
                currentCooldown = parentSensor.maxCooldown,

                grassSensorProbability = newGrassSensorProb,
                animalSensorProbability = newAnimalSensorProb,

                currentSensor = null,

                greenWeight = parentSensor.greenWeight,
                orangeWeight = parentSensor.orangeWeight,
                purpleWeight = parentSensor.purpleWeight,
                pinkWeight = parentSensor.pinkWeight,

                huntThreshod = parentSensor.huntThreshod,
                compoundHuntThresholdRate = parentSensor.compoundHuntThresholdRate,
            });


            
            float preferenceScaler = (newGrassSensorProb - 0.5f) / 0.5f; // (-1 to 1)
            int matureThreshold = (int)(20 + initAnimalConfig.adultAgeError * preferenceScaler); // e.g. 20 + 3 * ((1 - 0.5) / 0.5)
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new AgeStage
            {
                matureThreshold = matureThreshold,
                agingThreshold = matureThreshold + (int)Math.Ceiling(initAnimalConfig.adultDuration * (1 + initAnimalConfig.agingError * preferenceScaler)),

                currentStage = AgeStageEnum.infant, // initial stage
            });

            


            // ReproductionCounter Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new ReproductionCounter
            {
                interval = SystemAPI.GetComponent<ReproductionCounter>(parentEntity).interval,
                currentCD = SystemAPI.GetComponent<ReproductionCounter>(parentEntity).interval,
            });


            // Family Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new Family
            {
                parent = parentEntity,
            });



            // Curiosity
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new Curiosity
            {
                probability = parentAnimal.curiousityProbability,
                maxTime = parentAnimal.curiousityMaxTime,
                remainTime = parentAnimal.curiousityMaxTime
            });



            // Set Color
            entityCommandBuffer.AddComponent(newSpawnedAnimal, new NeedsColorInitialization
            {
                grassProb = newGrassSensorProb,
                animalProb = newAnimalSensorProb
            });
            /*
            if (state.EntityManager.HasBuffer<LinkedEntityGroup>(newSpawnedAnimal))
            {
                var linkedEntityGroupBuffer = state.EntityManager.GetBuffer<LinkedEntityGroup>(newSpawnedAnimal);

                // Assuming the child entity is the second entity in the LinkedEntityGroup
                Entity childEntity = linkedEntityGroupBuffer[1].Value;


                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(childEntity))
                {
                    // food preference
                    AnimalSensor animalSensor = state.EntityManager.GetComponentData<AnimalSensor>(newSpawnedAnimal);
                    float grassProb = newGrassSensorProb;
                    float animalProb = newAnimalSensorProb;



                    // Define the colors
                    float4 herbivorousColor = new float4(1.0f, 0.8f, 0.0f, 1.0f); // eat grass --- Yellow (FFCB00)
                    float4 carnivorousColor = new float4(1.0f, 0.3f, 0.3f, 1.0f); // eat meat  --- Red (FF4C4C)
                    float4 medianColor = new float4(0.76f, 1.0f, 1.0f, 1.0f); // median value(0.5) --- Cyan (C2FFFF)



                    // Interpolate between the three colors
                    float4 newAnimalColor;
                    if (grassProb > 0.5f)
                    {
                        // Blend between grassColor and medianColor
                        newAnimalColor = math.lerp(medianColor, herbivorousColor, (grassProb - 0.5f) * 2);
                    }
                    else
                    {
                        // Blend between animalColor and medianColor
                        newAnimalColor = math.lerp(carnivorousColor, medianColor, grassProb * 2);
                    }





                    var baseColor = state.EntityManager.GetComponentData<URPMaterialPropertyBaseColor>(childEntity);
                    baseColor.Value = newAnimalColor;
                    state.EntityManager.SetComponentData(childEntity, baseColor);
                }
            }
            */

        }
    }
}

public struct Family : IComponentData
{
    public Entity parent;
}

public struct NeedsColorInitialization : IComponentData
{
    public float grassProb;
    public float animalProb;
}