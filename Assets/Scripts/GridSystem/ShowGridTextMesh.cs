using System;
using Unity.Entities;
using UnityEngine;

public class ShowGridTextMesh : MonoBehaviour
{


    private int width;  // x
    private int height; // y
    private int gridCellSize;

    private Vector3 originPosition = Vector3.zero;

    private TextMesh[,] textMeshArray;


    private void Start()
    {
        GridUpdateSystem gridUpdateSystem =
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GridUpdateSystem>();

        gridUpdateSystem.OnGridsInit += InitGridMesh;
        gridUpdateSystem.OnOneGridCellValueChanged += SetGridText;
        gridUpdateSystem.OnAllGridCellValueChanged += RefreshAllText;
    }




    private void InitGridMesh(int width, int height, int gridCellSize, DynamicBuffer<GridCell> gridCellBuffer)
    {
        this.width = width;
        this.height = height;
        this.gridCellSize = gridCellSize;


        textMeshArray = new TextMesh[width, height];



        GameObject wordTextContainer = new GameObject("Word Text Container");
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                /*
                string printText;

                GridCell? cell = GridBufferUtils.GetGridCell(gridCellBuffer, width, x, y);

                if (!cell.HasValue) // if output is null (invalid)
                    printText = "Invalid";
                else
                {
                    printText = cell.Value.soilMoisture.ToString();
                }
                */



                //GridHelper.CreateWorldText($"{x.ToString("00")}{y.ToString("00")}", wordTextContainer.transform, GetWorldPosition(x, y) + new Vector3(gridCellSize, 0, gridCellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                textMeshArray[x, y] = GridHelper.CreateWorldText("", wordTextContainer.transform, GetWorldPosition(x, y) + new Vector3(gridCellSize, 0, gridCellSize) * 0.5f, 30, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    // convert grid position to world positon
    private Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * gridCellSize + originPosition;
    }

    private void SetGridText(int x, int y, string text)
    {
        textMeshArray[x, y].text = text;


        try
        {
            if (int.Parse(text) > 0)
            {
                textMeshArray[x, y].color = Color.yellow;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    private void RefreshAllText(DynamicBuffer<GridCell> gridCellBuffer)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string printText;

                GridCell? cell = GridBufferUtils.GetGridCell(gridCellBuffer, width, x, y);

                if (!cell.HasValue) // if output is null (invalid)
                    printText = "Invalid";
                else
                {
                    printText = cell.Value.soilMoisture.ToString();
                }



                SetGridText(x, y, printText);
            }
        }
    }
}
