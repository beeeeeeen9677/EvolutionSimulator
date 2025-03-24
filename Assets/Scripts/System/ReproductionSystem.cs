using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        AnimalAspect parentAnimal = SystemAPI.GetAspect<AnimalAspect>(parentEntity);

        parentAnimal.ConsumeFixedEnergy(0.2f * parentAnimal.maxEnergy);        // Consume 20% energy


        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();

        int numberToSpawn = 2;

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
            entityCommandBuffer.SetComponent(newSpawnedAnimal, SystemAPI.GetComponent<Energy>(parentEntity));


            // AnimalSensor Component
            AnimalSensor parentSensor = SystemAPI.GetComponent<AnimalSensor>(parentEntity);
            entityCommandBuffer.SetComponent(newSpawnedAnimal, new AnimalSensor
            {
                size = parentSensor.size,

                warningRange = parentSensor.warningRange,

                maxCooldown = parentSensor.maxCooldown,
                currentCooldown = parentSensor.maxCooldown,

                grassSensorProbability = parentSensor.grassSensorProbability,
                animalSensorProbability = 1 - parentSensor.grassSensorProbability,

                currentSensor = null,

                greenWeight = parentSensor.greenWeight,
                orangeWeight = parentSensor.orangeWeight,
                purpleWeight = parentSensor.purpleWeight,
                pinkWeight = parentSensor.pinkWeight,
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
        }
    }
}

public struct Family : IComponentData
{
    public Entity parent;
}
 