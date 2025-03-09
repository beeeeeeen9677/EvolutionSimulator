using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class InitLakeSystem : SystemBase
{

    protected override void OnCreate()
    {
        RequireForUpdate<InitLakeConfig>();
        RequireForUpdate<InitGrassConfig>();
        RequireForUpdate<InitGridSystemConfig>();
    }


    protected override void OnUpdate()
    {
        Enabled = false; // run for one loop only

        // grid system config
        RefRW<InitGridSystemConfig> initGridSystemConfig = SystemAPI.GetSingletonRW<InitGridSystemConfig>();
        Entity initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
        var gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
        int gridBufferWidth = initGridSystemConfig.ValueRO.width;
        int gridCellSize = initGridSystemConfig.ValueRO.gridCellSize;
        Vector3 gridSystemOrigin = initGridSystemConfig.ValueRO.originPosition;




        // grass and lake config
        InitLakeConfig initLakeConfig = SystemAPI.GetSingleton<InitLakeConfig>();
        InitGrassConfig initGrassConfig = SystemAPI.GetSingleton<InitGrassConfig>();
        Entity initGrassConfigEntity = SystemAPI.GetSingletonEntity<InitGrassConfig>();
        var grassTypeBuffer = EntityManager.GetBuffer<GrassPrefabElement>(initGrassConfigEntity);









        /* Old field size
        int fieldSize = initLakeConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }
        */

        // spawn lake & coresponding grass
        for (int i = 0; i < initLakeConfig.initLakeNumber; i++)
        {
            // ensure number of gird cells larger than total number of Objects (grasses & trees & )
            // if number of objects exceeding remaining quotas, stop adding new objects
            if (initGridSystemConfig.ValueRO.remainingGrids <= 0)
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



            Entity newSpawnedLake = EntityManager.Instantiate(initLakeConfig.lakePrefab);

            LakeProperty lakeProperty = SystemAPI.GetComponent<LakeProperty>(newSpawnedLake);


            // place new spawned lake into an empty cell
            GridCell allocatedGridCell = GridBufferUtils.SetObjectOnGridRandomly(gridCellBuffer, newSpawnedLake, lakeProperty.initSoilMoisture);




            float3 lakePosition = GridBufferUtils.GetWorldPosition(allocatedGridCell.X, allocatedGridCell.Y, gridCellSize, gridSystemOrigin);
            //float3 lakePosition = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0, UnityEngine.Random.Range(-fieldSize, fieldSize));

            SystemAPI.SetComponent(newSpawnedLake, new LocalTransform
            {
                Position = lakePosition,
                Rotation = quaternion.identity,
                Scale = 1
            });




            // get surrounding grids of the lake (center grid) by lake range
            List<GridCell> surroundingGrids = GridBufferUtils.GetSurroundingGridCells(gridCellBuffer, gridBufferWidth, lakeProperty.lakeRange, allocatedGridCell.X, allocatedGridCell.Y);


            // set grid density of center grid
            if (allocatedGridCell.soilDensity < 4)
            {
                // set to 4
                GridBufferUtils.ModifyGridDensity(gridCellBuffer, gridBufferWidth, allocatedGridCell.X, allocatedGridCell.Y, 4);
            }

            // set grid density of surrounding grids
            foreach (GridCell gridCell in surroundingGrids)
            {
                if(gridCell.soilDensity < 4)
                {
                    // set to 4
                    GridBufferUtils.ModifyGridDensity(gridCellBuffer, gridBufferWidth, gridCell.X, gridCell.Y, 4);
                }
            }


            // remove all occupied (not available) grids 
            surroundingGrids.RemoveAll(gridCell => gridCell.storingObject != Entity.Null);



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
                //Entity newSpawnedGrass = state.EntityManager.Instantiate(initGrassConfig.grassPrefab);   //old code
                Entity newSpawnedGrass = EntityManager.Instantiate(grassTypeBuffer[grassIndex].grassPrefabs);




                // place new spawned grass into surrounding grid cell of this lake
                float3 newGrassPosition;
                // check whether no more available surrounding grids
                if (surroundingGrids.Count <= 0)
                {
                    Debug.Log("No more empty surrounding grids (grass) ");
                    break;
                }
                else
                {
                    // if still have available surrounding grids, pick one grid cell randomly
                    GridCell pickedGridCell = surroundingGrids[UnityEngine.Random.Range(0, surroundingGrids.Count)];
                    surroundingGrids.Remove(pickedGridCell);

                    // attach the grass entity to grid cell
                    GridBufferUtils.SetGridCell(gridCellBuffer, gridBufferWidth, pickedGridCell.X, pickedGridCell.Y, newSpawnedGrass);

                    newGrassPosition = GridBufferUtils.GetWorldPosition(pickedGridCell.X, pickedGridCell.Y, gridCellSize, gridSystemOrigin);
                }



                SystemAPI.SetComponent(newSpawnedGrass, new LocalTransform
                {
                    /*
                    Position = new 
                        float3(UnityEngine.Random.Range(-lakeProperty.lakeRange, lakeProperty.lakeRange), 
                        0.5f, 
                        UnityEngine.Random.Range(-lakeProperty.lakeRange, lakeProperty.lakeRange)) 
                        + lakePosition,
                    */
                    Position = newGrassPosition,
                    Rotation = quaternion.identity,
                    Scale = 1
                });

                // set grass property
                GrassProperties grassPropertyData = SystemAPI.GetComponent<GrassProperties>(newSpawnedGrass);
                float randomMaxSize = UnityEngine.Random.Range(0.7f, 1f);
                /*
                SystemAPI.SetComponent(newSpawnedGrass, new GrassProperties
                {
                    currentSize = 0,
                    maxSize = randomMaxSize,
                    provideEnergy = grassPropertyData.provideEnergy,
                    activated = grassPropertyData.activated,
                });
                */
                RefRW<GrassProperties> grassProperties = SystemAPI.GetComponentRW<GrassProperties>(newSpawnedGrass);
                grassProperties.ValueRW.currentSize = 0;
                grassProperties.ValueRW.maxSize = randomMaxSize;
                grassProperties.ValueRW.provideEnergy = grassPropertyData.provideEnergy;
                grassProperties.ValueRW.activated = grassPropertyData.activated;

            }
        }
    }



}

