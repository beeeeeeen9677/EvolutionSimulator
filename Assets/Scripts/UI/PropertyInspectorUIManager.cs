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


    public static PropertyInspectorUIManager instance;



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

    private AnimalAspect _animalAspect;

    private Action OnSelectedEntityLost;
    #endregion


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

        ReassignAnimalAspect();

        // add different properties onto inspector
        InspectEntityID();
        InspectPosition();
        InspectEnergy();
        InspectHungerState();
        InspectReproductionCD();
        InspectMovement();
        InspectSensor();
        InspectTarget();
        InspectThreat();
        InspectSprint();
        InspectAge();
        InspectCell();
        InspectSize();
    }



    public void ReassignAnimalAspect()     // reassign the variable storing AnimalAspect due to ObjectDisposedException
    {
        if (selectedEntity != Entity.Null)
            _animalAspect = entityManager.GetAspect<AnimalAspect>(selectedEntity);
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
        GameObject newPropertyContainerGameObj = Instantiate(propertyContainerPrefab, inspectorScrollViewContent);
        PropertyContainer propertyContainer = newPropertyContainerGameObj.GetComponent<PropertyContainer>();
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


    private void InspectHungerState()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Hunger");

        propertyContainer.valueUpdateFunc = () => {

            HungerStatus hungerStatus = entityManager.GetComponentData<HungerStatus>(selectedEntity);

            return 
            $"threshold: {hungerStatus.hungryThreshold.ToString("0.0")}\n" +
            $"state: {hungerStatus.currentState}";
        };
    }



    private void InspectReproductionCD()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Reproduction");

        propertyContainer.valueUpdateFunc = () => {
            ReproductionCounter counter = entityManager.GetComponentData<ReproductionCounter>(selectedEntity);
            return
            $"interval: {counter.interval.ToString("0.00")}\n" +
            $"current: {counter.currentCD.ToString("0.00")}";
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
            int numberOfCell = entityManager.GetComponentData<Cell>(selectedEntity).numberOfNormalCell;
            return
            // actual speed
            $"current: {(_animalAspect.GetMoveSpeed()).ToString("0.0")}\n" +
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
            $"size: {_animalAspect.GetSensorSize().ToString("0.0")}\n" +
            $"finding: {sensor.currentSensor}\n" +
            $"warning range: {_animalAspect.GetWarningRange().ToString("0.0")}\n" +
            $"Cooldown: \n" +
            $" - current: {sensor.currentCooldown.ToString("0.0")}\n" +
            $" - max: {sensor.maxCooldown.ToString("0.0")}\n"+
            $"Probability: \n" +
            $" - grass: {sensor.grassSensorProbability.ToString("0.000")}\n" +
            $" - animal: {sensor.animalSensorProbability.ToString("0.000")}";
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

            Target tempTarget = entityManager.GetComponentData<Target>(selectedEntity);

            int tempTargetID = tempTarget.targetEntity.Index;

            return 
            "id: " + (tempTargetID == 0 ? "empty" : tempTargetID.ToString()) + "\n" + 
            $"max chase time: {tempTarget.maxChaseTime.ToString("0.0")}s\n" +
            $"remaining time: {tempTarget.remainChaseTime.ToString("0.0")}s";
            //+ "\n" + entityManager.GetComponentData<LocalTransform>(tempTargetEntity).Position;
        };
    }


    private void InspectThreat()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Threat");
       
        propertyContainer.valueUpdateFunc = () => {


            int tempThreatID = entityManager.GetComponentData<Threat>(selectedEntity).threatEntity.Index;

            return "id: " + (tempThreatID == 0 ? "empty" : tempThreatID.ToString());
            //+ "\n" + entityManager.GetComponentData<LocalTransform>(tempTargetEntity).Position;
        };
    }


    private void InspectSprint()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Sprint");

        propertyContainer.valueUpdateFunc = () => {

            Sprint sprint = entityManager.GetComponentData<Sprint>(selectedEntity);

            return
            $"sprint speed: {_animalAspect.sprintSpeed.ToString("0.00")}\n" +
            $"counter: {sprint.effectCounter.ToString("0.00")}s\n" +
            $"total: {sprint.effectTotalTime.ToString("0.00")}s";
        };


    }





    private void InspectAge()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Age");
        
        propertyContainer.valueUpdateFunc = () => {
            AgeStage ageStage = entityManager.GetComponentData<AgeStage>(selectedEntity);
            return 
            $"age: {(int)entityManager.GetComponentData<Age>(selectedEntity).currentAge}\n" +
            $"stage: {ageStage.currentStage.ToString()}\n" +
            $"cutoff: {ageStage.matureThreshold} / {ageStage.agingThreshold}";
        };
    }
   

    private void InspectCell()
    {
        PropertyContainer propertyContainer = CreateNewPropertyContainer("Cell");

        propertyContainer.valueUpdateFunc = () => {

            return
            $"normal: {entityManager.GetComponentData<Cell>(selectedEntity).numberOfNormalCell}\n" +
            $"meat: {entityManager.GetComponentData<Cell>(selectedEntity).numberOfMeatCell}";
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
