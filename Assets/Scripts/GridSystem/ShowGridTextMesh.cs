using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowGridTextMesh : MonoBehaviour
{


    [SerializeField]
    private int width;  // x
    [SerializeField]
    private int height; // y
    [SerializeField]
    private int gridCellSize;

    private Vector3 originPosition = Vector3.zero;


    private void Start()
    {
        GameObject wordTextContainer = new GameObject("Word Text Container");
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridHelper.CreateWorldText($"{x.ToString("00")}{y.ToString("00")}", wordTextContainer.transform, GetWorldPosition(x, y) + new Vector3(gridCellSize, 0, gridCellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
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
}
