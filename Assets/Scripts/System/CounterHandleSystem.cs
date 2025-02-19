using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using static UnityEngine.EventSystems.EventTrigger;



public partial struct CounterHandleSystem : ISystem
{
    private int BatchSize;// number of animals to be processed in each batch
    private int totalAnimals;
    private int totalBatches;




    public void OnCreate(ref SystemState state)
    {
        //state.EntityManager.CreateEntity(typeof(AnimalBatch));
     
    }


    public void OnUpdate(ref SystemState state)
    {
        RefRW<AnimalBatch> animalBatch = animalBatch = SystemAPI.GetSingletonRW<AnimalBatch>();
        if(BatchSize == 0)
        {
            BatchSize = animalBatch.ValueRO.BatchSize;
            Debug.Log("BatchSize is Assigned: " + BatchSize);
        }

        totalAnimals = state.EntityManager.CreateEntityQuery(typeof(AnimalTag)).CalculateEntityCount();
        totalBatches = (totalAnimals + BatchSize - 1) / BatchSize; // ensure that we round up to the nearest whole number.

        int startIndex = animalBatch.ValueRO.CurrentBatchIndex * BatchSize;
        int endIndex = math.min(startIndex + BatchSize, totalAnimals);





        var query = state.EntityManager.CreateEntityQuery(typeof(AnimalTag));
        var animalEntities = query.ToEntityArray(Allocator.Temp);
        for (int ci = startIndex; ci < endIndex && ci < animalEntities.Length; ci++)
        {
            //AnimalAspect currentAnimal = SystemAPI.GetAspect<AnimalAspect>(animalEntities[ci]);
            Entity currentEntity = animalEntities[ci];



            // Find target
            SystemAPI.SetComponentEnabled<SensorTriggerTag>(currentEntity, true);




            // Reproduction
            GrowUpAspect animal = SystemAPI.GetAspect<GrowUpAspect>(animalEntities[ci]);
            RefRO< Energy > energy = SystemAPI.GetComponentRO<Energy>(animalEntities[ci]);
            //if (animal.currentStage != AgeStageEnum.mature)
            if (animal.currentStage == AgeStageEnum.infant)  // only generate offspring if not at infant stage
                continue;

            // if current energy more than 50% of max energy, born offspring
            if (energy.ValueRO.currentEnergy >= 0.7f * energy.ValueRO.maxEnergy)
            {
                if (animal.IsReadyToGenerateOffspring(SystemAPI.Time.DeltaTime))
                {
                    SystemAPI.SetComponentEnabled<ReproductionTag>(currentEntity, true);
                }
            }


        }




        animalEntities.Dispose();



        animalBatch.ValueRW.CurrentBatchIndex = (animalBatch.ValueRO.CurrentBatchIndex + 1) % totalBatches;
        if (animalBatch.ValueRO.CurrentBatchIndex == 0)
        {
            animalBatch.ValueRW.CycleCount++;
        }

        //Debug.Log(animalBatch.ValueRO.CycleCount);

    }
    


}
