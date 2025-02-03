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
        // Initiation
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
            int initDiffusionTime = 0; // Do 3 times at first
            for (int i = 0; i < initDiffusionTime; i++)
            {
                DiffuseSoilWater(gridCellBuffer, gridBufferWidth);
            }


            OnAllGridCellValueChanged?.Invoke(gridCellBuffer);


            initialized = true;
        }



        // Usual update
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);

            DiffuseSoilWater(gridCellBuffer, gridBufferWidth);
        }


    }

    private void DiffuseSoilWater(DynamicBuffer<GridCell> gridCellBuffer, int gridBufferWidth)
    {
        // Stack for storing all modification records, excute them outside the loop
        List<ModifyGridMoistureRecord> modifyMoistureStack = new List<ModifyGridMoistureRecord>();


        foreach (GridCell centerGridCell in gridCellBuffer)
        {
            if (centerGridCell.soilMoisture >= 2) // at least 2 unit of moisture to diffuse
            {
                // get surrounding grids of the lake (center grid) by lake range
                List<GridCell> surroundingGridList = GridBufferUtils.GetSurroundingGridCells(gridCellBuffer, gridBufferWidth, 1, centerGridCell.X, centerGridCell.Y);

                //Debug.Log("Moisture: " + centerGridCell.soilMoisture + "   " + surroundingGridList.Count);




                // Set the soil moisture of surrounding grids to half of center grid
                foreach (GridCell surroundingGridCell in surroundingGridList)
                {
                    // the moisture of grid to be diffused should not be higher than half of moisture of center grid
                    if (surroundingGridCell.soilMoisture >= centerGridCell.soilMoisture / 2)
                        continue;

                    //GridBufferUtils.SetGridCell(gridCellBuffer, gridBufferWidth, surroundingGridCell.X, surroundingGridCell.Y, surroundingGridCell.storingObject, centerGridCell.soilMoisture / 2);
                    modifyMoistureStack.Add(new ModifyGridMoistureRecord(surroundingGridCell, centerGridCell.soilMoisture / 2));

                    //OnOneGridCellValueChanged?.Invoke(surroundingGridCell.X, surroundingGridCell.Y, (centerGridCell.soilMoisture / 2).ToString());
                }

            }
        }


        // execute all stacked modification records
        GridBufferUtils.ExecuteStackedRecord(gridCellBuffer, gridBufferWidth, modifyMoistureStack);

        OnAllGridCellValueChanged?.Invoke(gridCellBuffer);
    }
}

// for storing records of soil moisture modification
public class ModifyGridMoistureRecord
{
    public GridCell gridCell { get; private set; } // grid cell to be modified
    public int moistureToBeAdded { get; private set; } // value of moisture to be added after modified


    public ModifyGridMoistureRecord(GridCell gridCell, int moistureToBeAdded)
    {
        this.gridCell = gridCell;
        this.moistureToBeAdded = moistureToBeAdded;
    }
}