using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitAnimalConfigAuthoring : MonoBehaviour
{
    public GameObject animalPrefab;
    public int initAnimalNumber; // number to spawn initially


    public class Baker : Baker<InitAnimalConfigAuthoring>
    {
        public override void Bake(InitAnimalConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitAnimalConfig
            {
                animalPrefab = GetEntity(authoring.animalPrefab, TransformUsageFlags.Dynamic),
                initAnimalNumber = authoring.initAnimalNumber
            });
        }
    }
}

public struct InitAnimalConfig : IComponentData
{
    public Entity animalPrefab; 
    public int initAnimalNumber; // number to spawn initially
}
