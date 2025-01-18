using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct InitLakeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitLakeConfig>();
        state.RequireForUpdate<InitGrassConfig>();
        state.RequireForUpdate<InitGridSystemConfig>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only


        RefRW<InitGridSystemConfig> initGridSystemConfig;
        initGridSystemConfig = SystemAPI.GetSingletonRW<InitGridSystemConfig>();
        Entity initGridSystemConfigEntity;
        initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
        var gridCellBuffer = state.EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);






        InitLakeConfig initLakeConfig = SystemAPI.GetSingleton<InitLakeConfig>();
        InitGrassConfig initGrassConfig = SystemAPI.GetSingleton<InitGrassConfig>();
        Entity initGrassConfigEntity = SystemAPI.GetSingletonEntity<InitGrassConfig>();
        var grassTypeBuffer = state.EntityManager.GetBuffer<GrassPrefabElement>(initGrassConfigEntity);


        int fieldSize = initLakeConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }

        // spawn lake & coresponding grass
        for (int i = 0; i < initLakeConfig.initLakeNumber; i++)
        {
            // ensure number of gird cells larger than total number of Objects (grasses & trees & )
            // if number of objects exceeding remaining quotas, stop adding new objects
            if(initGridSystemConfig.ValueRO.remainingGrids <= 0)
            {
                // if no more available grids ...
                Debug.Log("No more available grids (Lake)");
                return;
            }
            else
            {
                initGridSystemConfig.ValueRW.remainingGrids--;
                //Debug.Log("Placed new object (Lake)");
            }



            Entity newSpawnedLake = state.EntityManager.Instantiate(initLakeConfig.lakePrefab);
            
            LakeProperty lakeProperty = SystemAPI.GetComponent<LakeProperty>(newSpawnedLake);


            // place new spawned lake into an empty cell
            GridCell allocatedGridCell = GridBufferUtils.SetObjectOnGridRandomly(gridCellBuffer, newSpawnedLake);




            float3 lakePosition = GridBufferUtils.GetWorldPosition(allocatedGridCell.X, allocatedGridCell.Y, initGridSystemConfig.ValueRO.gridCellSize, initGridSystemConfig.ValueRO.originPosition);
            //float3 lakePosition = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0, UnityEngine.Random.Range(-fieldSize, fieldSize));

            SystemAPI.SetComponent(newSpawnedLake, new LocalTransform
            {
                Position = lakePosition,
                Rotation = quaternion.identity,
                Scale = 1
            });




            // spawn grass for current lake
            int grassNumOfCurrentLake = lakeProperty.numberOfGrass;
            for (int j = 0; j < grassNumOfCurrentLake; j++)
            {
                // ensure number of gird cells larger than total number of Objects (grasses & trees & )
                // if number of objects exceeding remaining quotas, stop adding new objects
                if (initGridSystemConfig.ValueRO.remainingGrids <= 0)
                {
                    // if no more available grids ...
                    Debug.Log("No more available grids (Grass)");
                    return;
                }
                else
                {
                    initGridSystemConfig.ValueRW.remainingGrids--;
                    //Debug.Log("Placed new object (Grass)");
                }



                // get random type of grasses
                int grassIndex = UnityEngine.Random.Range(0, grassTypeBuffer.Length);
                Entity newSpawnedGrass = state.EntityManager.Instantiate(grassTypeBuffer[grassIndex].grassPrefabs);

                // place new spawned grass into surrounding cell of this lake
                // ...
                // ... 
                // ...
                // ...


                //Entity newSpawnedGrass = state.EntityManager.Instantiate(initGrassConfig.grassPrefab);   //old code


                SystemAPI.SetComponent(newSpawnedGrass, new LocalTransform
                {
                    Position = new 
                        float3(UnityEngine.Random.Range(-lakeProperty.lakeRange, lakeProperty.lakeRange), 
                        0.5f, 
                        UnityEngine.Random.Range(-lakeProperty.lakeRange, lakeProperty.lakeRange)) 
                        + lakePosition,
                    Rotation = quaternion.identity,
                    Scale = 1
                });

                // set grass property
                GrassProperties grassPropertyData = SystemAPI.GetComponent<GrassProperties>(newSpawnedGrass);
                float randomMaxSize = UnityEngine.Random.Range(0.7f, 1f);
                SystemAPI.SetComponent(newSpawnedGrass, new GrassProperties
                {
                    currentSize = 0,
                    maxSize = randomMaxSize,
                    provideEnergy = grassPropertyData.provideEnergy,
                    activated = grassPropertyData.activated,
                });
                
            }
        }
    }
}

