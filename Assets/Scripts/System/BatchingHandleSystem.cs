using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(ConfigUpdateSystem))]
public partial struct BatchingHandleSystem : ISystem
{
    private int BatchSize;// number of animals to be processed in each batch
    private int totalNumOfAnimals;
    private int totalNumOfBatches;

    private NativeArray<Entity> animalEntities; // will be updated in every Cycle 




    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimalBatch>();

        //state.EntityManager.CreateEntity(typeof(AnimalBatch));
        animalEntities = new NativeArray<Entity>(0, Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        // Dispose of the animalEntities array
        if (animalEntities.IsCreated)
        {
            animalEntities.Dispose();
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        RefRW<AnimalBatch> animalBatch = SystemAPI.GetSingletonRW<AnimalBatch>();
        if (BatchSize == 0)
        {
            BatchSize = animalBatch.ValueRO.BatchSize;
            Debug.Log("BatchSize is Assigned: " + BatchSize);
        }

        // ==== ADDED SAFETY CHECK ====
        if (!animalEntities.IsCreated)
        {
            animalBatch.ValueRW.CurrentBatchIndex = 0;
            return;
        }

        // update batching parameters in the start of every Cycle
        if (animalBatch.ValueRO.CurrentBatchIndex == 0)
        {
            totalNumOfAnimals = state.EntityManager.CreateEntityQuery(typeof(AnimalTag)).CalculateEntityCount();
            totalNumOfBatches = (totalNumOfAnimals + BatchSize - 1) / BatchSize;

            if (totalNumOfAnimals != 0)
            {
                // ==== MODIFIED: SAFER ARRAY RECREATION ====
                using (var query = state.EntityManager.CreateEntityQuery(typeof(AnimalTag)))
                {
                    var newEntities = query.ToEntityArray(Allocator.Persistent);
                    if (animalEntities.IsCreated)
                    {
                        animalEntities.Dispose();
                    }
                    animalEntities = newEntities;
                    ShuffleArray(); // shuffle animalEntities
                }
            }
            else
            {
                animalBatch.ValueRW.CurrentBatchIndex = 0;
                return;
            }
        }

        // ==== ADDED BOUNDS CHECKING ====
        int startIndex = animalBatch.ValueRO.CurrentBatchIndex * BatchSize;
        if (startIndex >= animalEntities.Length)
        {
            animalBatch.ValueRW.CurrentBatchIndex = 0;
            return;
        }

        int endIndex = math.min(startIndex + BatchSize, totalNumOfAnimals);
        endIndex = math.min(endIndex, animalEntities.Length); // Additional safety check

        // ==== MODIFIED PROCESSING LOOP WITH MORE CHECKS ====
        for (int ci = startIndex; ci < endIndex; ci++)
        {
            Entity currentEntity = animalEntities[ci];

            // Skip if entity is invalid
            if (!state.EntityManager.Exists(currentEntity))
                continue;

            try
            {
                SystemAPI.SetComponentEnabled<SensorTriggerTag>(currentEntity, true);

                // ==== ADDED NULL CHECK ====
                if (!SystemAPI.HasComponent<Age>(currentEntity) ||
                    !SystemAPI.HasComponent<Energy>(currentEntity))
                    continue;

                GrowUpAspect animal = SystemAPI.GetAspect<GrowUpAspect>(currentEntity);
                RefRO<Energy> energy = SystemAPI.GetComponentRO<Energy>(currentEntity);

                if (animal.currentStage == AgeStageEnum.infant)
                    continue;

                if (energy.ValueRO.currentEnergy >= 0.3f * energy.ValueRO.maxEnergy)
                {
                    if (animal.IsReadyToGenerateOffspring(SystemAPI.Time.DeltaTime))
                    {
                        SystemAPI.SetComponentEnabled<ReproductionTag>(currentEntity, true);
                    }
                }
            }
            catch (ArgumentException e)
            {
                Debug.LogError($"Batching System Error: {e.Message}");
                continue;
            }
        }

        // ==== MODIFIED BATCH INDEX UPDATE WITH SAFETY ====
        if (totalNumOfBatches > 0 && animalEntities.Length > 0)
        {
            animalBatch.ValueRW.CurrentBatchIndex = (animalBatch.ValueRO.CurrentBatchIndex + 1) % totalNumOfBatches;
        }
        else
        {
            animalBatch.ValueRW.CurrentBatchIndex = 0;
        }

        // Rest of the cycle/day logic 
        // if finished one cycle
        if (animalBatch.ValueRO.CurrentBatchIndex == 0)
        {
            animalBatch.ValueRW.CycleCount++;



            // if finished one day
            if (animalBatch.ValueRO.CycleCount % animalBatch.ValueRO.CycleOfDay == 0) // 24 cycles = 1 day
            {
                // water diffusion

                // Get a reference to GridUpdateSystem
                GridUpdateSystem gridUpdateSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GridUpdateSystem>();

                // Call grid update (water diffusion) function from GridUpdateSystem
                gridUpdateSystem.UpdateGridCellsInformation(animalBatch.ValueRO.CycleCount / animalBatch.ValueRO.CycleOfDay);


                // plant growth
                //var grassFlag = SystemAPI.GetSingletonRW<GrassGrowUpSystemFlag>();
                //grassFlag.ValueRW.flag = true;
            }
        }
    }
    /* Deprecated Update function
    public void OnUpdate(ref SystemState state)
    {
        //return;

        RefRW<AnimalBatch> animalBatch = SystemAPI.GetSingletonRW<AnimalBatch>();
        if (BatchSize == 0)
        {
            BatchSize = animalBatch.ValueRO.BatchSize;
            Debug.Log("BatchSize is Assigned: " + BatchSize);
        }




        // update batching parameters in the start of every Cycle
        if (animalBatch.ValueRO.CurrentBatchIndex == 0)
        {
            totalNumOfAnimals = state.EntityManager.CreateEntityQuery(typeof(AnimalTag)).CalculateEntityCount();
            totalNumOfBatches = (totalNumOfAnimals + BatchSize - 1) / BatchSize; // ensure that we round up to the nearest whole number.


            if (totalNumOfAnimals != 0)
            {

                
                //Debug.Log("BatchSize: " + BatchSize);
                //Debug.Log("totalNumOfAnimals: " + totalNumOfAnimals);
                //Debug.Log("totalNumOfBatches: " + totalNumOfBatches);
                


                // Recreate and shuffle the animalEntities array at the start of each cycle
                if (animalEntities.IsCreated)
                {
                    animalEntities.Dispose();
                }



                var query = state.EntityManager.CreateEntityQuery(typeof(AnimalTag));
                animalEntities = query.ToEntityArray(Allocator.Persistent);

                // Shuffle the array
                ShuffleArray();
            }
        }
        
        //else
        //{
         //   //Debug.Log("Batching: No animal exist in current update, wait for next update ");
        //}
        

        int startIndex = animalBatch.ValueRO.CurrentBatchIndex * BatchSize;
        int endIndex = math.min(startIndex + BatchSize, totalNumOfAnimals);


        for (int ci = startIndex; ci < endIndex && ci < animalEntities.Length; ci++)
        {
            //AnimalAspect currentAnimal = SystemAPI.GetAspect<AnimalAspect>(animalEntities[ci]);
            Entity currentEntity = animalEntities[ci];


            try
            {
                // Find target
                SystemAPI.SetComponentEnabled<SensorTriggerTag>(currentEntity, true);




                // Reproduction
                GrowUpAspect animal = SystemAPI.GetAspect<GrowUpAspect>(animalEntities[ci]);
                RefRO<Energy> energy = SystemAPI.GetComponentRO<Energy>(animalEntities[ci]);
                //if (animal.currentStage != AgeStageEnum.mature)
                if (animal.currentStage == AgeStageEnum.infant)  // only generate offspring if not at infant stage
                    continue;

                // if current energy more than 50% of max energy, born offspring
                if (energy.ValueRO.currentEnergy >= 0.3f * energy.ValueRO.maxEnergy)
                {
                    if (animal.IsReadyToGenerateOffspring(SystemAPI.Time.DeltaTime))
                    {
                        SystemAPI.SetComponentEnabled<ReproductionTag>(currentEntity, true);
                    }
                }
            }
            catch (ArgumentException)
            {
                Debug.LogError($"Batching System: Entity {currentEntity.Index} ArgumentException");
            }


        }




        //animalEntities.Dispose(); no need to dispose while using Permantent Native Array


        // add one to the current batch index, set back to 0 if it exceeds the total number of batches
        if (totalNumOfBatches != 0)
        {
            animalBatch.ValueRW.CurrentBatchIndex = (animalBatch.ValueRO.CurrentBatchIndex + 1) % totalNumOfBatches;
        }
        else
        {
            animalBatch.ValueRW.CurrentBatchIndex = 0;
        }

        // if finished one cycle
        if (animalBatch.ValueRO.CurrentBatchIndex == 0)
        {
            animalBatch.ValueRW.CycleCount++;



            // if finished one day
            if (animalBatch.ValueRO.CycleCount % animalBatch.ValueRO.CycleOfDay == 0) // 24 cycles = 1 day
            {
                // water diffusion

                // Get a reference to GridUpdateSystem
                GridUpdateSystem gridUpdateSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GridUpdateSystem>();

                // Call grid update (water diffusion) function from GridUpdateSystem
                gridUpdateSystem.UpdateGridCellsInformation(animalBatch.ValueRO.CycleCount / animalBatch.ValueRO.CycleOfDay);


                // plant growth
                //var grassFlag = SystemAPI.GetSingletonRW<GrassGrowUpSystemFlag>();
                //grassFlag.ValueRW.flag = true;
            }
        }

        //Debug.Log(animalBatch.ValueRO.CycleCount);

    }
    */

    private void ShuffleArray()
    {
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        for (int i = 0; i < animalEntities.Length; i++)
        {
            int j = random.NextInt(i, animalEntities.Length);
            var temp = animalEntities[i];
            animalEntities[i] = animalEntities[j];
            animalEntities[j] = temp;
        }
    }


}

