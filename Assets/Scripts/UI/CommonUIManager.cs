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

    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        animalBatchEntity = entityManager.CreateEntityQuery(typeof(AnimalBatch)).GetSingletonEntity();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateCycleText();
    }

    private void UpdateCycleText()
    {
        AnimalBatch animalBatch = entityManager.GetComponentData<AnimalBatch>(animalBatchEntity);
        cycleText.text = animalBatch.CycleCount.ToString();
        dayText.text = (animalBatch.CycleCount / animalBatch.CycleOfDay).ToString(); // 24 cycle as 1 day
    }
}
