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

        // grid system config
        RefRW<InitGridSystemConfig> initGridSystemConfig;
        initGridSystemConfig = SystemAPI.GetSingletonRW<InitGridSystemConfig>();
        Entity initGridSystemConfigEntity;
        initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
        var gridCellBuffer = state.EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
        int gridBufferWidth = initGridSystemConfig.ValueRO.width;
        int gridCellSize = initGridSystemConfig.ValueRO.gridCellSize;
        Vector3 gridSystemOrigin = initGridSystemConfig.ValueRO.originPosition;




        // grass and lake config
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




            float3 lakePosition = GridBufferUtils.GetWorldPosition(allocatedGridCell.X, allocatedGridCell.Y, gridCellSize, gridSystemOrigin);
            //float3 lakePosition = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0, UnityEngine.Random.Range(-fieldSize, fieldSize));

            SystemAPI.SetComponent(newSpawnedLake, new LocalTransform
            {
                Position = lakePosition,
                Rotation = quaternion.identity,
                Scale = 1
            });




            // get surrounding grids of the lake (center grid) by lake range
            List<GridCell> surroundingGrids = GetSurroundingGridCells(gridCellBuffer, gridBufferWidth, lakeProperty.lakeRange, allocatedGridCell.X, allocatedGridCell.Y);

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
                Entity newSpawnedGrass = state.EntityManager.Instantiate(grassTypeBuffer[grassIndex].grassPrefabs);




                // place new spawned grass into surrounding grid cell of this lake
                float3 newGrassPosition;
                // check whether no more available surrounding grids
                if(surroundingGrids.Count <= 0)
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


    private List<GridCell> GetSurroundingGridCells(DynamicBuffer<GridCell> buffer, int arrayWidth,  int range, int center_X, int center_Y)
    {
        List<GridCell> gridCellList = new List<GridCell>();

        // surrounding grids
        // 1. upper part    (starting from the first row)  (left part in game scene)
        for (int row = range; row > 0; row--) // loop (column in game scene)
        {
            int x = center_X - row;

            for (int y = center_Y - (range - row); y < center_Y + (range - row) + 1; y++)
            {
                AddGridIntoList(buffer, arrayWidth, gridCellList, x, y);
            }
        }
        // 2. middle part
        for (int y = center_Y - range; y < center_Y + range + 1; y++)
        {
            if (y == center_Y) // skip center gird
                continue;

            AddGridIntoList(buffer, arrayWidth, gridCellList, center_X, y);
        }
        // 3. lower part    (starting from the first row)  (right part in game scene)
        for (int row = 1; row <= range; row++) // loop (column in game scene)
        {
            int x = center_X + row;

            for (int y = center_Y - (range - row); y < center_Y + (range - row) + 1; y++)
            {
                AddGridIntoList(buffer, arrayWidth, gridCellList, x, y);
            }
        }


        return gridCellList;
    }

    private void AddGridIntoList(DynamicBuffer<GridCell> buffer, int arrayWidth, List<GridCell> gridCellList, int x, int y)
    {
        GridCell? cell = GridBufferUtils.GetGridCell(buffer, arrayWidth, x, y);
        if (cell.HasValue) // if output is not null (valid)
        {
            gridCellList.Add(cell.Value);
        }
    }
}

