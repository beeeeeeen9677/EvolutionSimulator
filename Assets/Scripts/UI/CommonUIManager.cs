using System;
using System.Linq.Expressions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class CommonUIManager : MonoBehaviour
{
    private Entity animalBatchEntity;
    private EntityManager entityManager;

    [SerializeField]
    private Text cycleText;
    [SerializeField]
    private Text dayText;
    [SerializeField]
    private Button InfoButton;

    
    private GameObject cycleTextContainer, dayTextContainer, gridTextMesh;





    private GridUpdateSystem gridUpdateSystem;



    // Start is called before the first frame update
    void Start()
    {

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        /*
        try
        {
            animalBatchEntity = entityManager.CreateEntityQuery(typeof(AnimalBatch)).GetSingletonEntity();
            cycleText.text = "start1.5";

        }
        catch (InvalidOperationException)
        {
            Debug.LogError("Common UI: Sub-Scene is still loading");
            cycleText.text = "Common UI: Sub-Scene is still loading";
        }
        cycleText.text = "start2";
        */

        InfoButton.onClick.AddListener(OnInfoButtonClick);

        cycleTextContainer = cycleText.transform.parent.gameObject;
        dayTextContainer = dayText.transform.parent.gameObject;


        gridUpdateSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GridUpdateSystem>();
        gridUpdateSystem.OnCycleExecuted += UpdateCycleText;


        cycleText.text = "starting";


    }


    private void UpdateCycleText(int cycleCount, int cycleOfDay)
    {

        cycleText.text = cycleCount.ToString();
        dayText.text = (cycleCount / cycleOfDay).ToString(); // 24 cycle as 1 day

        /*
        try
        {
            if (animalBatchEntity == Entity.Null)
            {
                animalBatchEntity = entityManager.CreateEntityQuery(typeof(AnimalBatch)).GetSingletonEntity();
            }
            AnimalBatch animalBatch = entityManager.GetComponentData<AnimalBatch>(animalBatchEntity);

            cycleText.text = animalBatch.CycleCount.ToString();
            dayText.text = (animalBatch.CycleCount / animalBatch.CycleOfDay).ToString(); // 24 cycle as 1 day
        }
        catch (ArgumentException)
        {
            Debug.LogError("Batching System ArgumentException");
            cycleText.text = "Batching System ArgumentException";         
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("Sub-Scene is still loading");
            cycleText.text = "Sub-Scene is still loading";
        }
        */
    }

    public void AssignGridMesh(GameObject go)
    {
        gridTextMesh = go;
    }

    private void OnInfoButtonClick() // toggle info game objects
    {
        // cycleTextContainer.SetActive(!cycleTextContainer.activeSelf);
        // dayTextContainer.SetActive(!dayTextContainer.activeSelf);
        gridTextMesh.SetActive(!gridTextMesh.activeSelf);
    }
}
