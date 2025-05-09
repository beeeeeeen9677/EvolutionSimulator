using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(ConfigUpdateSystem))]
public partial struct EnergySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Energy>();
    }

    public void OnUpdate(ref SystemState state)
    {
        /*
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new EnergyHandleJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),

        }.ScheduleParallel();
        */

        
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged); 
        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach (AnimalAspect animal in SystemAPI.Query<AnimalAspect>())
        {
            animal.ConsumeEnergy(deltaTime);


            // dead if no more energy
            if (animal.GetEnergy() <= 0)
            {


                // clear target's threat
                if (animal.IsTargetExist())
                {
                    // if target is an animal, clear target's threat
                    if (SystemAPI.HasComponent<AnimalTag>(animal.GetTargetEntity()))
                    {
                        AnimalAspect targetAnimal = SystemAPI.GetAspect<AnimalAspect>(animal.GetTargetEntity());
                        targetAnimal.ClearThreat(animal.entity);
                        Debug.Log("Threat clear since threat died");
                    }
                        

                }



                // add nutrition to soil
                AddNutrientToSoil(animal, ref state);


                // destroy itself
                ecb.DestroyEntity(animal.entity);


                // Release habitat vacancy
                ReleaseVacancy(animal.habitatProperty);

                //animal.entity = Entity.Null;

                // add ToBeDestroyed Component and System later
            }
        }
        
    }

    private void ReleaseVacancy(HabitatProperty? habitatProperty)
    {
        if (!habitatProperty.HasValue)
            return;

        HabitatProperty hb = (HabitatProperty)habitatProperty;

        hb.vacancy -= 1;
        if (hb.vacancy > hb.capacity)
        {
            hb.vacancy = hb.capacity;
        }
    }

    private void AddNutrientToSoil(AnimalAspect animal, ref SystemState state)
    {
        Debug.Log("Add Nutrient To Soil");

        // grid system config var
        InitGridSystemConfig initGridSystemConfig = SystemAPI.GetSingleton<InitGridSystemConfig>();
        Entity initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
        DynamicBuffer<GridCell> gridCellBuffer = state.EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
        int gridBufferWidth = initGridSystemConfig.width;
        int gridCellSize = initGridSystemConfig.gridCellSize;
        Vector3 gridSystemOrigin = initGridSystemConfig.originPosition;

        // get world position of the died animal
        int x, z;
        GridBufferUtils.GetCoordinateByWorldPosition(animal.position, out x, out z, gridCellSize, gridSystemOrigin);

        int range = 1;

        // get surrounding grid cells in square shape range
        List<GridCell> surroundingGridList = GridBufferUtils.GetSurroundingGridCellsInSquare(gridCellBuffer, gridBufferWidth, range, x, z);

        Debug.Log("Length: "+ surroundingGridList.Count);

        // Set the soil nutrient of surrounding grids according to the animal's size
        foreach (GridCell surroundingGridCell in surroundingGridList)
        {
            float nutrient = animal.currentSize;
            GridBufferUtils.AddGridNutrient(gridCellBuffer, gridBufferWidth, surroundingGridCell.X, surroundingGridCell.Y, nutrient);
            //SetGridCell(buffer, arrayWidth, recordGridCell.Value.X, recordGridCell.Value.Y, recordGridCell.Value.storingObject, modifiedMoisture);
        }
    }

  
}


public partial struct EnergyHandleJob : IJobEntity
{
    public float deltaTime;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(AnimalAspect animal, [EntityIndexInQuery] int sortKey)
    {
        animal.ConsumeEnergy(deltaTime);

        // dead if no more energy
        if (animal.GetEnergy() <= 0)
        {

            ecb.DestroyEntity(sortKey, animal.entity);

            //animal.entity = Entity.Null;

            // add ToBeDestroyed Component and System later
        }

   
    }
}
