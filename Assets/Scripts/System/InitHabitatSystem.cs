using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class InitHabitatSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<InitHabitatConfig>();
        RequireForUpdate<InitGridSystemConfig>();
    }


    protected override void OnUpdate()
    {
        Enabled = false; // run for one loop only



        SpawnShelter();
    }

    private void SpawnShelter(int treeToBeSpawned = 0, float3 providedPosition = default)
    {
        RefRW<InitGridSystemConfig> initGridSystemConfig = SystemAPI.GetSingletonRW<InitGridSystemConfig>();
        Entity initGridSystemConfigEntity;
        initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
        var gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);




        InitLakeConfig initLakeConfig = SystemAPI.GetSingleton<InitLakeConfig>();
        int fieldSize = initLakeConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }



        // for setting MAX & MIN size value
        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();
        float minInitValue = initAnimalConfig.minInitSize * 1.5f;
        float maxInitValue = initAnimalConfig.maxInitSize * 1.5f;
        Debug.Log($"InitAnimalConfig   min: {minInitValue}  max: {maxInitValue}");

        InitHabitatConfig initHabitatConfig = SystemAPI.GetSingleton<InitHabitatConfig>();

        float maxRadius = initHabitatConfig.maxRadius; // 30
        float minRadius = initHabitatConfig.minRadius; // 15
        float effectOfHiding = initHabitatConfig.effectOfHiding;


        if (treeToBeSpawned == 0)
            treeToBeSpawned = initHabitatConfig.numberOfHabitat;



        for (int i = 0; i < treeToBeSpawned; i++)
        {

            // ensure number of gird cells larger than total number of Objects (grasses & trees & )
            // if number of objects exceeding remaining quotas, stop adding new objects
            if (initGridSystemConfig.ValueRO.remainingGrids <= 0)
            {
                // if no more available grids ...
                Debug.Log("No more available grids (Habitat/Shelter)");
                return;
            }
            else
            {
                initGridSystemConfig.ValueRW.remainingGrids--;
                //Debug.Log("Placed new object (Lake)");
            }




            Entity newSpawnedHabitat = EntityManager.Instantiate(initHabitatConfig.habitatPrefab);

            // place new spawned habitat/shelter into an empty cell
            GridCell allocatedGridCell = GridBufferUtils.SetObjectOnGridRandomly(gridCellBuffer, newSpawnedHabitat);



            // random position
            float3 habitatPosition = GridBufferUtils.GetWorldPosition(allocatedGridCell.X, allocatedGridCell.Y, initGridSystemConfig.ValueRO.gridCellSize, initGridSystemConfig.ValueRO.originPosition);
            //float3 habitatPosition = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0, UnityEngine.Random.Range(-fieldSize, fieldSize));
            SystemAPI.SetComponent(newSpawnedHabitat, new LocalTransform
            {
                Position = habitatPosition,
                Rotation = quaternion.identity,
                Scale = 1
            });


            // set MAX & MIN size value
            float midValue = UnityEngine.Random.Range(minInitValue, maxInitValue);
            float minSize = midValue - initHabitatConfig.interval / 2;
            float maxSize = midValue + initHabitatConfig.interval / 2;

            float radius = UnityEngine.Random.Range(minRadius, maxRadius);
            SystemAPI.SetComponent(newSpawnedHabitat, new HabitatProperty
            {
                minSize = minSize,
                maxSize = maxSize,
                radius = radius,
                effectOfHiding = effectOfHiding,
            });


            HabitatProperty habitatProperty = SystemAPI.GetComponent<HabitatProperty>(newSpawnedHabitat);
            /*
            Debug.Log($"mid: {midValue} interval: {initHabitatConfig.interval / 2}    min: {minSize}    max: {maxSize}\n" +
                $"Habitat {i} - min: {habitatProperty.minSize}    max: {habitatProperty.maxSize}");
            */
            //Debug.Log($"Habitat {i}:  min: {habitatProperty.minSize}    max: {habitatProperty.maxSize}");




            // create domain effect by radius
            Entity newDomainEffect = EntityManager.Instantiate(initHabitatConfig.domainEffectPrefab);
            SystemAPI.SetComponent(newDomainEffect, new LocalTransform
            {
                Position = habitatPosition,
                Rotation = quaternion.identity,
                Scale = radius * 2,
            });



            // get surrounding grids of the lake (center grid) by lake range
            int gridBufferWidth = initGridSystemConfig.ValueRO.width;

            List<GridCell> surroundingGrids = GridBufferUtils.GetSurroundingGridCellsInSquare(gridCellBuffer, gridBufferWidth, 4, allocatedGridCell.X, allocatedGridCell.Y);

            // set grid density
            foreach (GridCell gridCell in surroundingGrids)
            {
                if (gridCell.soilDensity < 2)
                {
                    // set to 4
                    GridBufferUtils.ModifyGridDensity(gridCellBuffer, gridBufferWidth, gridCell.X, gridCell.Y, 2);
                }
            }
        }
    }

    public void ReadyToSpawnTree(float3 position)
    {
        SpawnShelter(1, position);
    }
}

