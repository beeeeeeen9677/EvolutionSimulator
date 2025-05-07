using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitHabitatConfigAuthoring : MonoBehaviour
{
    [SerializeField]
    private GameObject habitatPrefab;
    [SerializeField]
    private int numberOfHabitat;
    [SerializeField]
    private float interval;     // value from min to max
    [SerializeField]
    private float maxRadius;
    [SerializeField]
    private float minRadius;
    [SerializeField] 
    private int capacity;
    [SerializeField]
    private float effectOfHiding;

    [SerializeField]
    private GameObject domainEffectPrefab;



    public class InitHabitatConfigBaker : Baker<InitHabitatConfigAuthoring>
    {
        public override void Bake(InitHabitatConfigAuthoring authoring)
        {
            int numOfHab = PlayerPrefs.GetInt("NumOfHab", authoring.numberOfHabitat);
            PlayerPrefs.SetInt("NumOfHab", numOfHab);

            float effectOfHiding = PlayerPrefs.GetFloat("EffectOfHiding", authoring.effectOfHiding);
            PlayerPrefs.SetFloat("EffectOfHiding", effectOfHiding);


            int capacity = PlayerPrefs.GetInt("Capacity", authoring.capacity);
            PlayerPrefs.SetInt("Capacity", capacity);



            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitHabitatConfig
            {
                habitatPrefab = GetEntity(authoring.habitatPrefab, TransformUsageFlags.None),
                numberOfHabitat = numOfHab,
                interval = authoring.interval,  
                maxRadius = authoring.maxRadius,
                minRadius = authoring.minRadius,
                capacity = capacity,
                effectOfHiding = effectOfHiding,
                domainEffectPrefab = GetEntity(authoring.domainEffectPrefab, TransformUsageFlags.None),
            });
        }
    }
}


public struct InitHabitatConfig : IComponentData
{
    public Entity habitatPrefab;
    public int numberOfHabitat;
    // value from min to max
    public float interval;
    public float maxRadius;
    public float minRadius;
    public float effectOfHiding; // a number between 0 - 1 to decide whether can be detected by enemy, larger better
    public int capacity; // number of animals that can live here

    public Entity domainEffectPrefab; // show the range (radius) of the habitat
}