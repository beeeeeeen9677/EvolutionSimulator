using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



[UpdateAfter(typeof(InitLakeSystem))]
public partial class GridUpdateSystem : SystemBase
{
    private bool initialized;


    public Action<int, int, int, DynamicBuffer<GridCell>> OnGridsInit;
    public Action<int, int, string> OnOneGridCellValueChanged;
    public Action<DynamicBuffer<GridCell>> OnAllGridCellValueChanged;




    RefRW<InitGridSystemConfig> initGridSystemConfig;
    Entity initGridSystemConfigEntity;
    Vector3 gridSystemOrigin;
    int gridBufferWidth;
    int gridCellSize;


    protected override void OnCreate()
    {
        RequireForUpdate<InitGridSystemConfig>();
        initialized = false;
    }
    protected override void OnUpdate()
    {
        // Initiation, execute only once
        if (!initialized)
        {
            // grid system config var
            initGridSystemConfig = SystemAPI.GetSingletonRW<InitGridSystemConfig>();
            initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
            DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
            gridBufferWidth = initGridSystemConfig.ValueRO.width;
            gridCellSize = initGridSystemConfig.ValueRO.gridCellSize;
            gridSystemOrigin = initGridSystemConfig.ValueRO.originPosition;



            // init grid mesh text
            OnGridsInit?.Invoke(gridBufferWidth, initGridSystemConfig.ValueRO.height, gridCellSize, gridCellBuffer);



            Debug.Log("Init Diffusion");

            // Init water diffusion
            int initDiffusionTime = 3; // Do 3 times at first
            for (int i = 0; i < initDiffusionTime; i++)
            {
                DiffuseSoilWater();
            }


            OnAllGridCellValueChanged?.Invoke(gridCellBuffer);


            initialized = true;
        }



        // Usual update
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);

            DiffuseSoilWater();
        }


    }

    public void DiffuseSoilWater()
    {
        Debug.Log("Diffuse Soil Water");

        DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);



        int diffusionMode = 2;



        // Stack for storing all modification records, excute them outside the loop
        List<ModifyGridMoistureRecord> modifyMoistureStack = new List<ModifyGridMoistureRecord>();


        foreach (GridCell centerGridCell in gridCellBuffer)
        {


            // Mode 1: > 2          Mode 2: >= 2
            if (IsValidToDiffuse(diffusionMode, centerGridCell.soilMoisture_flooredValue)) // at least larger than 2 unit of moisture to diffuse
            {
                // get surrounding grids of the lake (center grid) by lake range
                List<GridCell> surroundingGridList = GridBufferUtils.GetSurroundingGridCells(gridCellBuffer, gridBufferWidth, 1, centerGridCell.X, centerGridCell.Y);

                //Debug.Log("Moisture: " + centerGridCell.soilMoisture_value + "   " + surroundingGridList.Count);




                // Set the soil moisture of surrounding grids to half of center grid
                foreach (GridCell surroundingGridCell in surroundingGridList)
                {


                    float moistureToBeAdded = 0;


                    #region Diffusion Mode 1 
                    if(diffusionMode == 1)
                    {
                        // Minecraft water diffusion logic, water of source grid will NOT be decreased

                        // the moisture of grid to be diffused should not be higher than half of moisture of center grid
                        if (surroundingGridCell.soilMoisture_value > centerGridCell.soilMoisture_flooredValue / 2)
                            continue;
                        // if moisture of surrounding ?? half of center
                        //  = :  add 1/4 center moisture
                        //  < :  add 1/2 center moisture
                        moistureToBeAdded = centerGridCell.soilMoisture_flooredValue /
                            ((surroundingGridCell.soilMoisture_value == centerGridCell.soilMoisture_flooredValue / 2) ? 4 : 2);

                    }
                    #endregion



                    #region Diffusion Mode 2
                    if(diffusionMode == 2)
                    {
                        // water of source grid will be decreased



                        // the moisture of grid to be diffused should not be higher than moisture of center grid
                        if (surroundingGridCell.soilMoisture_value > centerGridCell.soilMoisture_flooredValue)
                            continue;
                        float diffusionScaler = 0.05f;

                        moistureToBeAdded = centerGridCell.soilMoisture_flooredValue * diffusionScaler;
                        // decrease water from the source
                        modifyMoistureStack.Add(new ModifyGridMoistureRecord(centerGridCell.X, centerGridCell.Y, -1 * moistureToBeAdded));
                    }
                    #endregion


                    if(moistureToBeAdded == 0)
                    {
                        Debug.Log("moisture To Be Added = 0");
                    }


                    //GridBufferUtils.SetGridCell(gridCellBuffer, gridBufferWidth, surroundingGridCell.X, surroundingGridCell.Y, surroundingGridCell.storingObject, centerGridCell.soilMoisture_flooredValue / 2);
                    modifyMoistureStack.Add(new ModifyGridMoistureRecord(surroundingGridCell.X, surroundingGridCell.Y, moistureToBeAdded));

                    //OnOneGridCellValueChanged?.Invoke(surroundingGridCell.X, surroundingGridCell.Y, (centerGridCell.soilMoisture_flooredValue / 2).ToString());
                }

            }
        }


        // execute all stacked modification records
        GridBufferUtils.ExecuteStackedRecord(gridCellBuffer, gridBufferWidth, modifyMoistureStack);

        OnAllGridCellValueChanged?.Invoke(gridCellBuffer);
    }


    private bool IsValidToDiffuse(int diffusionMode, int moistureValue)
    {
        switch (diffusionMode)
        {
            // Mode 1: > 2          Mode 2: >= 2
            // at least larger than 2 unit of moisture to diffuse
            case 1:
                if (moistureValue > 2)
                {
                    //validToDiffuse = true;
                    return true;
                }
                break;

            case 2:
                if (moistureValue >= 2)
                {
                    //validToDiffuse = true;
                    return true;
                }
                break;

            default:
                Debug.Log("Invalid Diffusion Mode: " + diffusionMode);
                break;
        }

        return false;
    }
}

// for storing records of soil moisture modification
public class ModifyGridMoistureRecord
{
    //public GridCell gridCell { get; private set; } // cannot use struct since it is value type
    public int X { get; private set; } // XY coordinate of grid cell to be modified
    public int Y { get; private set; }
    public float moistureToBeAdded { get; private set; } // value of moisture to be added after modified


    public ModifyGridMoistureRecord(int X, int Y, float moistureToBeAdded)
    {
        this.X = X;
        this.Y = Y;
        this.moistureToBeAdded = moistureToBeAdded;
    }
}