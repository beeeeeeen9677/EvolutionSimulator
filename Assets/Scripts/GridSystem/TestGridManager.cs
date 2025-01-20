using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class TestGridManager : MonoBehaviour
{


    [SerializeField]
    private Text modeText;
    [SerializeField]
    private GameObject rangeContainer;
    [SerializeField]
    private InputField rangeInputField;
    private int mode;


    [Header("Grid SetUp")]
    public int width;
    public int height;


    private TestGrid grids;
    // Start is called before the first frame update
    private void Start()
    {
        ChangeMode(1);
        rangeInputField.text = "3"; // DEFAULT
        grids = new TestGrid(width, height, 10f, new Vector3(-20, 0, 0));
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            /*
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane; // or any other distance you need
            Debug.Log(mousePos);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            Debug.Log(worldPosition);
            */

            // Use raycast in 3D scene
            /*
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 worldPosition = hit.point;
                Debug.Log(worldPosition);
                grid.SetValue(worldPosition, 56);
            }
            */
            if(mode == 1) // change selected grid value randomly
            {
                grids.SetValue(GetMouseRaycastPosition(), Random.Range(1, 50));
            }
            else if(mode == 2) // get grids (change value) by range
            {
                int range = int.Parse(rangeInputField.text);
                int center_X, center_Y;
                grids.GetXZ(GetMouseRaycastPosition(), out center_X, out center_Y); // get x y coordinate of selected grid


                // change all grids' value to zero
                grids.ResetAllGrids();

                // change selected grid(s)' value to specific number



                // surrounding grids
                // 1. upper part    (starting from the first row)  (left part in game scene)
                for (int row = range;  row > 0; row--) // loop (column in game scene)
                {
                    int x = center_X - row;

                    for(int y = center_Y - (range - row); y < center_Y + (range - row) + 1; y++)
                    {
                        grids.Increment(x, y);
                    }
                }
                // 2. middle part
                for (int y = center_Y - range; y < center_Y + range + 1; y++)
                {
                    if(y == center_Y) // skip center gird
                        continue;

                    grids.Increment(center_X, y);
                }
                // 3. lower part    (starting from the first row)  (right part in game scene)
                for (int row = 1; row <= range; row++) // loop (column in game scene)
                {
                    int x = center_X + row;

                    for (int y = center_Y - (range - row); y < center_Y + (range - row) + 1; y++)
                    {
                        grids.Increment(x, y);
                    }
                }


                // center grid
                grids.Increment(center_X, center_Y, 10);
            }
        }

        // show value of that grid
        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log("Value: " + grids.GetValue(GetMouseRaycastPosition()));
        }


        // change mode
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeMode(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeMode(2);
        }
    }


    private Vector3 GetMouseRaycastPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPosition = hit.point;
        }

        return hit.point;
    }

    private void ChangeMode(int mode)
    {
        this.mode = mode;

        modeText.text = "Mode: " + mode.ToString();

        rangeContainer.SetActive(mode == 2);
    }

    
}
