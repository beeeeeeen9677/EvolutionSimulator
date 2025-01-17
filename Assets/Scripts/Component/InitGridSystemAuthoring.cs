using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitGridSystemAuthoring : MonoBehaviour
{
    [SerializeField]
    private int width;  // x
    [SerializeField]
    private int height; // y
    [SerializeField]
    private int gridCellSize;



    public class InitGridSystemBaker : Baker<InitGridSystemAuthoring>
    {
        public override void Bake(InitGridSystemAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitGridSystemConfig
            {
                width = authoring.width,
                height = authoring.height,
                gridCellSize = authoring.gridCellSize,
                remainingGrids = authoring.width * authoring.height, // number of total grids
            });

            // for storing the 2D Grid Cells
            DynamicBuffer<GridCell> buffer = AddBuffer<GridCell>(entity);
            for (int x = 0; x < authoring.width; x++)
            {
                for (int y = 0; y < authoring.height; y++)
                {
                    buffer.Add(new GridCell { X = x, Y = y, Value = int.Parse($"{x}{y}") });
                }

            }
        }
    }
}


public struct InitGridSystemConfig : IComponentData
{
    public int width;  // x
    public int height; // y
    public int gridCellSize;
    public int remainingGrids; // number of empty grids (no object was placed on it)
}

// ref type cannot be used in ECS component, 
// use Dynamic Buffer instead
public struct GridCell : IBufferElementData
{
    // coordinate of the Grid Cell
    public int X;
    public int Y;
    // data storing in the Grid Cell
    public int Value;
}


public static class GridBufferUtils
{
    // Get/Set the dynamic buffer by XY coordinate
    public static GridCell GetGridCell(DynamicBuffer<GridCell> buffer, int width, int x, int y)
    {
        int index = x * width + y;
        return buffer[index];
    }

    public static void SetGridCell(DynamicBuffer<GridCell> buffer, int width, int x, int y, GridCell cell)
    {
        int index = x * width + y;
        buffer[index] = cell;
    }
}

public partial struct TestInitGridSystem : ISystem
{


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitGridSystemConfig>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Show ALL
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InitGridSystemConfig initGridSystemConfig;
            Entity initGridSystemConfigEntity;
            initGridSystemConfig = SystemAPI.GetSingleton<InitGridSystemConfig>();
            initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();


            DynamicBuffer<GridCell> gridCellBuffer = state.EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);


            string output = "";
            for(int x = 0; x < initGridSystemConfig.width; x++)
            {
                for(int y = 0; y < initGridSystemConfig.height; y++)
                {
                    output += GridBufferUtils.GetGridCell(gridCellBuffer, initGridSystemConfig.width, x, y).Value + " ";
                }
            }


            Debug.Log(output);
        }

        // Update Random Element
        if (Input.GetKeyDown(KeyCode.E))
        {
            InitGridSystemConfig initGridSystemConfig;
            Entity initGridSystemConfigEntity;
            initGridSystemConfig = SystemAPI.GetSingleton<InitGridSystemConfig>();
            initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();


            DynamicBuffer<GridCell> gridCellBuffer = state.EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);


            int randIndex = UnityEngine.Random.Range(0, gridCellBuffer.Length);

            gridCellBuffer[randIndex] = new GridCell { X = gridCellBuffer[randIndex].X, Y = gridCellBuffer[randIndex].Y, Value = DateTime.Now.Second };
        }
    }
}