using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;





public enum PlaceObjectType
{
    Lake = 0,
    Tree = 1,
    Grass = 2
}




public class PlaceObjectManager : MonoBehaviour
{
    public static PlaceObjectManager instance;

    [SerializeField] private GameObject lakePlaceHolder, treePlaceHolder;
    private GameObject[] placeHolders;
    private GameObject currentPlaceHolder;

    private bool activated;

    // Systems
    private InitLakeSystem initLakeSystem;
    private InitHabitatSystem initHabitatSystem;



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
        placeHolders = new GameObject[] { lakePlaceHolder, treePlaceHolder, null };


        initLakeSystem =
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<InitLakeSystem>();

        initHabitatSystem =
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<InitHabitatSystem>();
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

        treePlaceHolder.SetActive(objectType == PlaceObjectType.Tree && value == true);

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
                    if (EventSystem.current.IsPointerOverGameObject()) // Pointer Over UI Element
                        return;

                    CreateNewObject(position);
                }
            }
        }

    }

    private void CreateNewObject(Vector3 position)
    {

        PlaceObjectType typeToBeAdded = (PlaceObjectType)Array.IndexOf(placeHolders, currentPlaceHolder);

        ToggleInspectorSystem(false, typeToBeAdded); // hide place holder


        switch (typeToBeAdded)
        {
            case PlaceObjectType.Lake:
                initLakeSystem.ReadyToSpawnLake(position);
                break;

            case PlaceObjectType.Tree:
                initHabitatSystem.ReadyToSpawnTree(position);
                break;

            default:
                break;
        }

    }
}
