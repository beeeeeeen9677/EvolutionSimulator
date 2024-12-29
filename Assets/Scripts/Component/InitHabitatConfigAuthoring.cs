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


    public class InitHabitatConfigBaker : Baker<InitHabitatConfigAuthoring>
    {
        public override void Bake(InitHabitatConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitHabitatConfig
            {
                habitatPrefab = GetEntity(authoring.habitatPrefab, TransformUsageFlags.None),
                numberOfHabitat = authoring.numberOfHabitat,
                interval = authoring.interval,  
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
}