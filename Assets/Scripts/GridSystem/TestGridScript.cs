using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGridScript : MonoBehaviour
{
    private Grid grid;
    // Start is called before the first frame update
    private void Start()
    {
        grid = new Grid(4, 2, 10f, new Vector3(-20, 0, 0));
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
            grid.SetValue(GetMouseRaycastPosition(), Random.Range(1, 50));
        }

        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log("Value: " + grid.GetValue(GetMouseRaycastPosition()));
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
}
