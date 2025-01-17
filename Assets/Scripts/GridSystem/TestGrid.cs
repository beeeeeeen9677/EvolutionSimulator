using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid 
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;


    private int[,] gridArray;
    private TextMesh[,] debugTextArray;


    public TestGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];
        debugTextArray = new TextMesh[width, height];



        //Debug.Log($"w: {width}, h: {height}");

        for (int x = 0; x < gridArray.GetLength(0); x++) // width
        {
            for (int z = 0; z < gridArray.GetLength(1); z++) // height
            {
                //Debug.Log(x+", "+y);
                debugTextArray[x, z] = GridHelper.CreateWorldText(gridArray[x, z].ToString(),null, GetWorldPosition(x, z) + new Vector3(cellSize, 0, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 100f);

            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);


        //SetValue(2, 1, 9677);
    }


    // convert grid position to world positon
    private Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }


    // convert world position to grid positon
    private void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    // set value of cell in the array
    public void SetValue(int x, int z, int value)
    {
        if (x < 0 || z < 0 || x >= width || z >= height) // validation
            return;

        gridArray[x, z] = value;
        debugTextArray[x, z].text = gridArray[x, z].ToString();
    }


    public void SetValue(Vector3 worldPosition, int Value)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z); 
        SetValue(x, z, Value);
    }

    public int GetValue(int x, int z)
    {
        if (x < 0 || z < 0 || x >= width || z >= height) // validation
            return 0;

        return gridArray[x, z];
    }



    public int GetValue(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);

        return GetValue(x, z);
    }
}



