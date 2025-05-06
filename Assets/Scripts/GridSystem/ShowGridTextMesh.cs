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
                textMeshArray[x, y] = GridHelper.CreateWorldText("", wordTextContainer.transform, GetWorldPosition(x, y) + new Vector3(gridCellSize, 0, gridCellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
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


            try
            {
                if (float.Parse(text.Split("n: ")[1]) <= 0) // nutrient
                {
                    // if nutrient is 0, check if moisture is greater than 0
                    if (float.Parse(text.Split("\n")[0]) > 0) // moisture
                    {
                        textMeshArray[x, y].color = Color.yellow;
                    }
                    else
                    {
                        textMeshArray[x, y].color = Color.white;
                        //text = "/";
                    }
                }
            }
            catch (Exception)
            {
                Debug.Log("SetGridText: the moisture value is invalid");
            }
        

        


        textMeshArray[x, y].text = text;
    }


    private void SetGridText(int x, int y, float m, float d, float n)
    {


        try
        {
            if (n <= 0) // nutrient
            {
                // if nutrient is 0, check if moisture is greater than 0
                if (m > 0) // moisture
                {
                    textMeshArray[x, y].color = Color.yellow;
                }
                else
                {
                    textMeshArray[x, y].color = Color.white;
                    //text = "/";
                }
            }
        }
        catch (Exception)
        {
            Debug.Log("SetGridText: the moisture value is invalid");
        }



        string printText = m.ToString("0.##") + "\n<color=white>d: " + d.ToString("0.##") + "</color>\nn: " + n.ToString("0.##");


        textMeshArray[x, y].text = printText;
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
                {
                    printText = "Invalid";
                    SetGridText(x, y, printText);
                }
                else
                {
                    //printText = cell.Value.soilMoisture_value + "\n<color=white>lv: " + cell.Value.soilMoisture_level + "</color>\nf: " + cell.Value.soilMoisture_flooredValue;
                    //printText = cell.Value.soilMoisture_value + "\n<color=white>d: " + cell.Value.soilDensity + "</color>\nn: " + cell.Value.soilNutrient;
                    SetGridText(x, y, cell.Value.soilMoisture_value, cell.Value.soilDensity, cell.Value.soilNutrient);

                }



            }
        }
    }
}
