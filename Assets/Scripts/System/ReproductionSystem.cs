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
            //if (animal.currentStage != AgeStageEnum.mature)
            if (animal.currentStage == AgeStageEnum.infant)  // only generate offspring if not at infant stage
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
        AnimalAspect parentAnimal = SystemAPI.GetAspect<AnimalAspect>(parentEntity);

        parentAnimal.ConsumeEnergy(0.2f * parentAnimal.maxEnergy);        // Consume 20% energy


        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();

        int numberToSpawn = 2;

        for (int i = 0; i < numberToSpawn; i++)
        {
            Debug.Log($"{parentEntity.Index} Born offspring: {i+1} of {numberToSpawn}");

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
            entityCommandBuffer.SetComponent(newSpawnedAnimal, SystemAPI.GetComponent<Energy>(parentEntity));


            // AnimalSensor Component
            AnimalSensor parentSensor = SystemAPI.GetComponent<AnimalSensor>(parentEntity);
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new AnimalSensor
            {
                size = parentSensor.size,

                warningRange = parentSensor.warningRange,

                maxCooldown = parentSensor.maxCooldown,
                currentCooldown = 0,

                grassSensorProbability = parentSensor.grassSensorProbability,
                animalSensorProbability = 1 - parentSensor.grassSensorProbability,

                currentSensor = null,
            });


            // ReproductionCounter Component
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new ReproductionCounter
            {
                interval = SystemAPI.GetComponent<ReproductionCounter>(parentEntity).interval,
                currentCD = SystemAPI.GetComponent<ReproductionCounter>(parentEntity).interval,
            }); 
        }
    }
}
