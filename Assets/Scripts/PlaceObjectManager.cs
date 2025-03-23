using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;





public enum PlaceObjectType
{
    Lake = 0,
    Tree = 1,
    Grass = 2
}




public class PlaceObjectManager : MonoBehaviour
{
    public static PlaceObjectManager instance;

    [SerializeField] private GameObject lakePlaceHolder;
    private GameObject[] placeHolders;
    private GameObject currentPlaceHolder;

    private bool activated;

    private InitLakeSystem initLakeSystem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    private void Start()
    {
        activated = false;

        lakePlaceHolder.SetActive(false);
        placeHolders = new GameObject[] { lakePlaceHolder, null, null };


        initLakeSystem =
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<InitLakeSystem>();
    }


    public void ToggleInspectorSystem(PlaceObjectType objectType)
    {
        bool value = !activated;
        HandleToggle(value, objectType);
    }

    public void ToggleInspectorSystem(bool value, PlaceObjectType objectType)
    {
        HandleToggle(value, objectType);
    }

    private void HandleToggle(bool value, PlaceObjectType objectType)
    {

        ActivatePlaceHolder(value, objectType);

        activated = value;


        if (value == false)
        {
            ToolsBarController.instance.ResetButtonState();
        }
    }



    private void ActivatePlaceHolder(bool value, PlaceObjectType objectType)
    {
        lakePlaceHolder.SetActive(objectType == PlaceObjectType.Lake && value == true);

        currentPlaceHolder = placeHolders[(int)objectType];
    }

    private void Update()
    {
        if (activated)
        {
            HandleMouseInput();
        }
    }


    private void HandleMouseInput()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //Debug.Log("Ray Hit:" + hit.collider.gameObject.name);

            if (hit.collider.CompareTag("Ground"))
            {
                //Debug.Log("Hit ground");
                Vector3 position = hit.point;
                currentPlaceHolder.transform.position = position;   
                if (Input.GetMouseButtonDown(0)) // interact with initLakeSystem
                {
                    ToggleInspectorSystem(false, (PlaceObjectType)Array.IndexOf(placeHolders, currentPlaceHolder));

                    initLakeSystem.ReadyToSpawnLake(position);
                }
            }
        }

    }
}
