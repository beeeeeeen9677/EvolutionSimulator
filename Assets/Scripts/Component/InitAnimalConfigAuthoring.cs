using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitAnimalConfigAuthoring : MonoBehaviour
{
    public GameObject animalPrefab;
    public int initAnimalNumber; // number to spawn initially
    public int fieldSize;

    #region animalInitSize
    // min & max value of INITIAL size of animal
    public float minInitSize;
    public float maxInitSize;
    #endregion


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
                minInitSize = authoring.minInitSize,
                maxInitSize = authoring.maxInitSize,
            });
        }
    }
}

public struct InitAnimalConfig : IComponentData
{
    public Entity animalPrefab; 
    public int initAnimalNumber; // number to spawn initially
    public int fieldSize;

    public float minInitSize;
    public float maxInitSize;
}
