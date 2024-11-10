using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public partial struct ReproductionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Energy>();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator); //better

        foreach ((GrowUpAspect animal, RefRO<Energy> energy) in SystemAPI.Query<GrowUpAspect, RefRO<Energy>>())
        {
            // only generate offspring in mature stage
            if (animal.currentStage != AgeStageEnum.mature)
                return;

            // if current energy more than 50% of max energy, born offspring
            if(energy.ValueRO.currentEnergy >= 0.5f * energy.ValueRO.maxEnergy)
            {
                if (animal.IsReadyToGenerateOffspring(SystemAPI.Time.DeltaTime))
                {
                    GenerateOffspring(animal.entity, entityCommandBuffer, ref state);
                }
            }
        }

        entityCommandBuffer.Playback(state.EntityManager);//execute the recorded commands
    }

    private void GenerateOffspring(Entity parentEntity, EntityCommandBuffer entityCommandBuffer, ref SystemState state)
    {
        Debug.Log($"{parentEntity.Index} Born offspring");

        AnimalAspect parentAnimal = SystemAPI.GetAspect<AnimalAspect>(parentEntity);

        // Consume 20% energy
        parentAnimal.ConsumeEnergy(0.2f * parentAnimal.maxEnergy);


        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();

        int numberToSpawn = 1;

        for (int i = 0; i < numberToSpawn; i++)
        {
            Entity newSpawnedAnimal = entityCommandBuffer.Instantiate(initAnimalConfig.animalPrefab);


            // SizeProperty Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, SystemAPI.GetComponent<SizeProperty>(parentEntity));


            // LocalTransform Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new LocalTransform
            {
                Position = new float3(
                    UnityEngine.Random.Range(parentAnimal.position.x - 3, parentAnimal.position.x + 3),  // x
                    parentAnimal.position.y,                                                       // y
                    UnityEngine.Random.Range(parentAnimal.position.z - 3, parentAnimal.position.z + 3)), // z
                Rotation = quaternion.identity,
                Scale = SystemAPI.GetComponent<SizeProperty>(parentEntity).initSize
            });


            // assign random value to speed
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new Movement
            {
                speed = UnityEngine.Random.Range(1, 6)
            });


            float maxEnergy = UnityEngine.Random.Range(80, 120);
            //maxEnergy = UnityEngine.Random.Range(18, 29);    // for test
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new Energy
            {
                maxEnergy = maxEnergy,
                currentEnergy = maxEnergy
            });


            float grassSensorProbability = UnityEngine.Random.Range(0f, 1f);
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new AnimalSensor
            {
                size = 20f,

                warningRange = 10f,

                maxCooldown = 3,
                currentCooldown = 0,

                grassSensorProbability = grassSensorProbability,
                animalSensorProbability = 1 - grassSensorProbability,

                currentSensor = null,
            });



            entityCommandBuffer.SetComponent(newSpawnedAnimal, new ReproductionCounter
            {
                interval = 10f,
                currentCD = 10f,
            });
        }
    }
}
