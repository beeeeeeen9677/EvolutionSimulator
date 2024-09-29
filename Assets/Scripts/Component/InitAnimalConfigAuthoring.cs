using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitAnimalConfigAuthoring : MonoBehaviour
{
    public GameObject animalPrefab;
    public int initAnimalNumber; // number to spawn initially
    public int fieldSize;


    public class InitAnimalConfigBaker : Baker<InitAnimalConfigAuthoring>
    {
        public override void Bake(InitAnimalConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitAnimalConfig
            {
                animalPrefab = GetEntity(authoring.animalPrefab, TransformUsageFlags.Dynamic),
                initAnimalNumber = authoring.initAnimalNumber,
                fieldSize = authoring.fieldSize,
            });
        }
    }
}

public struct InitAnimalConfig : IComponentData
{
    public Entity animalPrefab; 
    public int initAnimalNumber; // number to spawn initially
    public int fieldSize;
}
