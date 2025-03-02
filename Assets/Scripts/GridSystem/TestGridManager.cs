using System;
using UnityEngine;
using UnityEngine.UI;

public class TestGridManager : MonoBehaviour
{


    [SerializeField]
    private Text modeText;
    private int mode;

    #region mode2
    [SerializeField]
    private GameObject rangeContainer;
    [SerializeField]
    private InputField rangeInputField;
    #endregion

    #region mode3
    [SerializeField]
    private GameObject mode3Container;
    [SerializeField]
    private InputField waterValueInputField;
    #endregion

    [Header("Grid SetUp")]
    public int width;
    public int height;





    private int global_center_X, global_center_Y;



    private TestGrid grids;
    // Start is called before the first frame update
    private void Start()
    {
        ChangeMode(1);
        rangeInputField.text = "3"; // DEFAULT
        waterValueInputField.text = "10";
        grids = new TestGrid(width, height, 10f, new Vector3(-20, 0, 0));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
            if (mode == 1) // change selected grid value randomly
            {
                grids.SetValue(GetMouseRaycastPosition(), UnityEngine.Random.Range(1, 50));
            }
            else if (mode == 2) // get grids (change value) by range
            {
                int range = int.Parse(rangeInputField.text);
                int center_X, center_Y;
                grids.GetXZ(GetMouseRaycastPosition(), out center_X, out center_Y); // get x y coordinate of selected grid


                // change all grids' value to zero
                grids.ResetAllGrids();

                // change selected grid(s)' value to specific number



                //GetRange(range, center_X, center_Y);
                GetSquareRange(range, center_X, center_Y);


                // center grid
                grids.Increment(center_X, center_Y, 10);
            }
            else if (mode == 3)
            {
                // change all grids' value to zero
                grids.ResetAllGrids();


                int waterValue = int.Parse(waterValueInputField.text);
                grids.GetXZ(GetMouseRaycastPosition(), out global_center_X, out global_center_Y); // get x y coordinate of selected grid

                grids.SetValue(global_center_X, global_center_Y, waterValue); // set the selected grid's value to sepific value
            }
        }





        // show value of that grid
        if (Input.GetMouseButtonDown(1))
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
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeMode(3);
        }


        // water defussion simulation
        if (mode == 3)
        {
            Mode3WaterDefussion();
        }

    }

    private void GetSquareRange(int range, int center_X, int center_Y)
    {
        // get square range of girds
        for (int x = center_X - range; x < center_X + range + 1; x++)
        {
            for (int y = center_Y - range; y < center_Y + range + 1; y++)
            {
                grids.Increment(x, y);
            }
        }
    }

    private void GetRange(int range, int center_X, int center_Y)
    {
        // surrounding grids
        // 1. upper part    (starting from the first row)  (left part in game scene)
        for (int row = range; row > 0; row--) // loop (column in game scene)
        {
            int x = center_X - row;

            for (int y = center_Y - (range - row); y < center_Y + (range - row) + 1; y++)
            {
                grids.Increment(x, y);
            }
        }
        // 2. middle part
        for (int y = center_Y - range; y < center_Y + range + 1; y++)
        {
            if (y == center_Y) // skip center gird
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

        mode3Container.SetActive(mode == 3);
    }



    private void Mode3WaterDefussion()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    if (grids.GetValue(w, h) <= 1)
                    {
                        // value not enough to defuse
                        continue;
                    }

                    float centerValueBeforeDefuse = grids.GetValue(w, h);
                    // up

                    // down

                    // left

                    // right



                }
            }
        }
    }
}
