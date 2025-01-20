using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.PackageManager;
using UnityEngine;

public class InitGridSystemAuthoring : MonoBehaviour
{
    [SerializeField]
    private int width;  // x
    [SerializeField]
    private int height; // y
    [SerializeField]
    private int gridCellSize;

    private Vector3 originPosition = Vector3.zero;


   
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
                originPosition = authoring.originPosition,
                remainingGrids = authoring.width * authoring.height, // number of total grids
            });
            Debug.Log("Total number of grids: " + authoring.width * authoring.height);

            // for storing the 2D Grid Cells
            DynamicBuffer<GridCell> buffer = AddBuffer<GridCell>(entity);
            for (int x = 0; x < authoring.width; x++)
            {
                for (int y = 0; y < authoring.height; y++)
                {
                    buffer.Add(new GridCell { X = x, Y = y, storingObject = Entity.Null });
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
    public Vector3 originPosition;
    public int remainingGrids; // number of empty grids (no object was placed on it)


    // functions
    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * gridCellSize + originPosition;
    }
}




// ref type cannot be used in ECS component, 
// use Dynamic Buffer instead
public struct GridCell : IBufferElementData
{
    // coordinate of the Grid Cell
    public int X;
    public int Y;
    // data storing in the Grid Cell
    public Entity storingObject;
}










// Utility Functions for Grid Cell dynamic buffer
public static class GridBufferUtils
{
    // Get/Set the dynamic buffer by XY coordinate
    public static GridCell? GetGridCell(DynamicBuffer<GridCell> buffer, int width, int x, int y)
    {
        int height = buffer.Length/width;
        if (x < 0 || y < 0 || x >= width || y >= height) // validation
        {
            Debug.Log("Grid system: Invalid X/Y coordinate");
            return null;
        }


        int index = x * width + y;
        return buffer[index];
    }

    // Set grid by buffer index
    public static void SetGridCell(DynamicBuffer<GridCell> buffer, int bufferIndex, GridCell cell)
    {
        buffer[bufferIndex] = cell;
    }
    public static void SetGridCell(DynamicBuffer<GridCell> buffer, int bufferIndex, Entity newEntity)     // Overload, set grid by Value
    {
        buffer[bufferIndex] = new GridCell { X = buffer[bufferIndex].X, Y = buffer[bufferIndex].Y, storingObject = newEntity };
    }



    // Set grid by Cell coordinate
    public static void SetGridCell(DynamicBuffer<GridCell> buffer, int width, int x, int y, GridCell cell)
    {
        int height = buffer.Length / width;
        if (x < 0 || y < 0 || x >= width || y >= height) // validation
        {
            Debug.Log("Grid system: Invalid X/Y coordinate");
            return;
        }



        int index = x * width + y;
        buffer[index] = cell;
    }
    public static void SetGridCell(DynamicBuffer<GridCell> buffer, int width, int x, int y, Entity newEntity)     // Overload, set grid by Value
    {
        int height = buffer.Length / width;
        if (x < 0 || y < 0 || x >= width || y >= height) // validation
        {
            Debug.Log("Grid system: Invalid X/Y coordinate");
            return;
        }


        int index = x * width + y;
        buffer[index] = new GridCell { X = buffer[index].X, Y = buffer[index].Y, storingObject = newEntity };
    }


    // place a new entity/object (shelter/grass/lake) on an empty grid
    public static GridCell SetObjectOnGridRandomly(DynamicBuffer<GridCell> buffer, Entity newEntity)
    {
        int bufferLenght = buffer.Length;
        // get random index
        int randIdx = -1;
        while(true)
        {
            randIdx = UnityEngine.Random.Range(0, bufferLenght);
            // loop until current random grid cell is empty
            if(buffer[randIdx].storingObject == Entity.Null)
            {
                //buffer[randIdx].storingObject = newEntity;
                SetGridCell(buffer, randIdx, newEntity);
                return buffer[randIdx];
            }
        }
    }



    public static Vector3 GetWorldPosition(int x, int z, int cellSize, Vector3 originPosition)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition + new Vector3(0.5f * cellSize, 0, 0.5f * cellSize);
    }







}

/*
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
*/