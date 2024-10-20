using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using static PropertyContainer;

public class PropertyInspectorUIManager : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private GameObject inspectorPanel;
    [SerializeField]
    private Button closeButton;


    [SerializeField]
    private Transform inspectorScrollViewContent; // for putting property container inside

    [SerializeField]
    private GameObject propertyContainerPrefab;
    //[SerializeField] private GameObject propertyValuePrefab; // put in property container, stores the value of that property



    public static Entity selectedEntity { get; private set; }
    public static EntityManager entityManager { get; private set; }

    private Action OnSelectedEntityLost;
    #endregion






    // Start is called before the first frame update
    private void Start()
    {
        UnitSelectionSystem unitSelectionSystem = 
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<UnitSelectionSystem>();

        unitSelectionSystem.OnAnimalSelected += SetSelectedEntityAndOpenInspector;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        closeButton.onClick.AddListener(() => CloseInspectorAndResetTarget());
    }


    private void SetSelectedEntityAndOpenInspector(Entity entity)
    {
        selectedEntity = entity;

        inspectorPanel.SetActive(true); // open inspector
        ClearAllChild(inspectorScrollViewContent);


        // add different properties onto inspector
        InspectEntityID();
        InspectPosition();
        InspectEnergy();
        InspectMovement();
        InspectSensor();
        InspectTarget();
        InspectAge();
        InspectCell();
        InspectSize();
    }


    private void CloseInspectorAndResetTarget()
    {
        selectedEntity = Entity.Null;
        ClearAllChild(inspectorScrollViewContent);
        inspectorPanel.SetActive(false);
    }


    // Deprecated UpdateInspectorUI()
    /*
    private void UpdateInspectorUI()
    {
        ClearAllChild(inspectorScrollViewContent);

        // check if entity is destroyed
        if (!entityManager.Exists(selectedEntity))
            selectedEntity = Entity.Null;

        // check if any entity is selected
        if (selectedEntity == Entity.Null)
            return;



        //Debug.Log("UI: " + entity.Index);

        
        


        
        // add ToBeDestroyed Component and System later
        try
        {
            // add different properties onto inspector
            InspectEntityID();
            InspectPosition();
            InspectEnergy();
            InspectMovement();
            InspectSensor();
            InspectTarget();
        }
        catch (ArgumentException e)
        {
            selectedEntity = Entity.Null;
            Debug.Log("Property Inspector: add ToBeDestroyed Component and System later");
            return;
        }
        
    }
    */

    private PropertyContainer CreateNewPropertyContainer(string propertyName)
    {
        GameObject newPropertyContainerObj = Instantiate(propertyContainerPrefab, inspectorScrollViewContent);
        PropertyContainer propertyContainer = newPropertyContainerObj.GetComponent<PropertyContainer>();
        propertyContainer.SetPropertyName(propertyName);

        OnSelectedEntityLost += propertyContainer.StopUpdating;

        return propertyContainer;
    }

    /*
    private void AddPropertyValueToContainer(PropertyContainer propertyContainer, string valueText)
    {
        propertyContainer.SetValueText(valueText);
    }
    public void SetValueUpdateDelegate(PropertyContainer propertyContainer, Func<string> func)
    {
        propertyContainer.valueUpdateFunc = () => func;
    }
   
    */


    //public delegate string UpadateContainerValue();



    #region Inspect Property Methods

    private void InspectEntityID()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Entity ID");

        /*
        UpadateContainerValue newValueUpdate = () => { 
            return $"value: {selectedEntity.Index}"; 
        };

        SetValueUpdateDelegate(propertyContainer, newValueUpdate);
        */

        propertyContainer.valueUpdateFunc = () => {
            return $"value: {selectedEntity.Index}";
        };
    }


    private void InspectPosition()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Position");

        /*
        UpadateContainerValue newValueUpdate = () => {
            return $"{((Vector3)entityManager.GetComponentData<LocalTransform>(selectedEntity).Position).ToString("F1")}";
        };

        SetValueUpdateDelegate(propertyContainer, newValueUpdate);
        */
        propertyContainer.valueUpdateFunc = () => {
            return $"value: {((Vector3)entityManager.GetComponentData<LocalTransform>(selectedEntity).Position).ToString("F1")}";
        };
    }


    private void InspectEnergy()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Energy");

        /*
        UpadateContainerValue newValueUpdate = () => {
            return $"current: {entityManager.GetComponentData<Energy>(selectedEntity).currentEnergy.ToString("0.0")}\n" +
            $"max: {entityManager.GetComponentData<Energy>(selectedEntity).maxEnergy.ToString("0.0")}";
        };

        SetValueUpdateDelegate(propertyContainer, newValueUpdate);
        */

        propertyContainer.valueUpdateFunc = () => {
            return $"current: {entityManager.GetComponentData<Energy>(selectedEntity).currentEnergy.ToString("0.0")}\n" +
                       $"max: {entityManager.GetComponentData<Energy>(selectedEntity).maxEnergy.ToString("0.0")}";
        };
    }


    private void InspectMovement()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Movement");

        /*
        UpadateContainerValue newValueUpdate = () => {
            return $"speed: {entityManager.GetComponentData<Movement>(selectedEntity).speed.ToString("0.0")}";
        };

        SetValueUpdateDelegate(propertyContainer, newValueUpdate);
        */
        propertyContainer.valueUpdateFunc = () => {
            float originalSpeed = entityManager.GetComponentData<Movement>(selectedEntity).speed;
            int numberOfCell = entityManager.GetComponentData<Cell>(selectedEntity).numberOfCell;
            return
            // actual speed
            $"current: {(originalSpeed * numberOfCell * 0.00001f).ToString("0.0")}\n" +
            // original speed
            $"original: {originalSpeed.ToString("0.0")}";
        };
    }


    private void InspectSensor()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Sensor");

        /*
        UpadateContainerValue newValueUpdate = () => {
            return $"size: {entityManager.GetComponentData<AnimalSensor>(selectedEntity).size.ToString("0.0")}";
        };

        SetValueUpdateDelegate(propertyContainer, newValueUpdate);
        */
        propertyContainer.valueUpdateFunc = () => {
            AnimalSensor sensor = entityManager.GetComponentData<AnimalSensor>(selectedEntity);
            return
            $"size: {sensor.size.ToString("0.0")}\n" +
            $"Cooldown: \n" +
            $" - current: {sensor.currentCooldown.ToString("0.0")}\n" +
            $" - max: {sensor.maxCooldown.ToString("0.0")}\n"+
            $"Probability: \n" +
            $" - grass: {sensor.grassSensorProb.ToString("0.000")}\n" +
            $" - animal: {sensor.animalSensorProb.ToString("0.000")}";
        };
    }


    private void InspectTarget()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Target");
        //AddPropertyValueToContainer(propertyContainer,
        //    $"position: {entityManager.GetComponentData<LocalTransform>(entityManager.GetComponentData<Target>(selectedEntity).targetEntity).Position}");


        /*
        UpadateContainerValue newValueUpdate = () => {
            return $"id: {entityManager.GetComponentData<Target>(selectedEntity).targetEntity.Index}";
        };

        SetValueUpdateDelegate(propertyContainer, newValueUpdate);
        */

        propertyContainer.valueUpdateFunc = () => {

            Entity tempTargetEntity = entityManager.GetComponentData<Target>(selectedEntity).targetEntity;

            int tempTargetID = entityManager.GetComponentData<Target>(selectedEntity).targetEntity.Index;

            return "id: " + (tempTargetID == 0 ? "empty" : tempTargetID.ToString());
            //+ "\n" + entityManager.GetComponentData<LocalTransform>(tempTargetEntity).Position;
        };
    }


    private void InspectAge()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Age");
        
        propertyContainer.valueUpdateFunc = () => {
            AgeStage ageStage = entityManager.GetComponentData<AgeStage>(selectedEntity);
            return $"age: {(int)entityManager.GetComponentData<Age>(selectedEntity).currentAge}\n" +
            $"stage: {ageStage.currentStage.ToString()}\n" +
            $"cutoff: {ageStage.matureThreshold} / {ageStage.agingThreshold}";
        };
    }


    private void InspectCell()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Cell");

        propertyContainer.valueUpdateFunc = () => {

            return $"number: {entityManager.GetComponentData<Cell>(selectedEntity).numberOfCell}";
        };
    }

    private void InspectSize()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Size");

        propertyContainer.valueUpdateFunc = () => {
            SizeProperty size = entityManager.GetComponentData<SizeProperty>(selectedEntity);
            return 
            $"current: {size.currentSize.ToString("0.00")}\n" +
            $"init: {size.initSize.ToString("0.00")}\n" +
            $"max: {size.maxSize.ToString("0.00")}";
        };
    }




    #endregion

    // old version
    /*
     * 
    private GameObject CreateNewPropertyContainer(string propertyName)
    {
        GameObject newPropertyContainer = Instantiate(propertyContainerPrefab, inspectorScrollViewContent);
        newPropertyContainer.GetComponentInChildren<Text>().text = propertyName;

        return newPropertyContainer;
    }

    private void AddPropertyValueToContainer(GameObject propertyContainer, string valueText)
    {
        GameObject propertyValue = Instantiate(propertyValuePrefab, propertyContainer.transform);
        propertyValue.GetComponent<Text>().text = valueText;
    }





    private void InspectEntityID()
    {
        GameObject propertyContainer = CreateNewPropertyContainer("Entity ID");
        GameObject propertyValue = Instantiate(propertyValuePrefab, propertyContainer.transform);
        propertyValue.GetComponent<Text>().text = $"value: {selectedEntity.Index}";
    }


    private void InspectPosition()
    {
        GameObject propertyContainer = CreateNewPropertyContainer("Position");
        AddPropertyValueToContainer(propertyContainer,
            $"{((Vector3)entityManager.GetComponentData<LocalTransform>(selectedEntity).Position).ToString("F1")}");
    }


    private void InspectEnergy()
    {
        GameObject propertyContainer = CreateNewPropertyContainer("Energy");
        AddPropertyValueToContainer(propertyContainer, 
            $"current: {entityManager.GetComponentData<Energy>(selectedEntity).currentEnergy.ToString("0.0")}");
        AddPropertyValueToContainer(propertyContainer,
          $"max: {entityManager.GetComponentData<Energy>(selectedEntity).maxEnergy.ToString("0.0")}");
    }


    private void InspectMovement()
    {
        GameObject propertyContainer = CreateNewPropertyContainer("Movement");
        AddPropertyValueToContainer(propertyContainer,
            $"current: {entityManager.GetComponentData<Movement>(selectedEntity).speed.ToString("0.0")}");
    }


    private void InspectSensor()
    {
        GameObject propertyContainer = CreateNewPropertyContainer("Sensor");
        AddPropertyValueToContainer(propertyContainer,
            $"size: {entityManager.GetComponentData<AnimalSensor>(selectedEntity).size.ToString("0.0")}");
    }

    private void InspectTarget()
    {
        GameObject propertyContainer = CreateNewPropertyContainer("Target");
        AddPropertyValueToContainer(propertyContainer,
            $"id: {entityManager.GetComponentData<Target>(selectedEntity).targetEntity.Index}");
        //AddPropertyValueToContainer(propertyContainer,
        //    $"position: {entityManager.GetComponentData<LocalTransform>(entityManager.GetComponentData<Target>(selectedEntity).targetEntity).Position}");
    }

    */



    private void Update()
    {
        // if the UI panel is not opened or no selected Entity
        if (!inspectorPanel.activeInHierarchy)
            return;


        //UpdateInspectorUI();  // deprecated
        CheckTargetEntityValidity();

    }

    private void CheckTargetEntityValidity()
    {
        // check if entity is destroyed
        if (!entityManager.Exists(selectedEntity))
            selectedEntity = Entity.Null;

        // check if any entity is selected
        if (selectedEntity == Entity.Null)
        {
            OnSelectedEntityLost?.Invoke();
            ClearAllChild(inspectorScrollViewContent);
        }
    }


    private void ClearAllChild(Transform container)
    {
        OnSelectedEntityLost = null; // clear all subscrition

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
