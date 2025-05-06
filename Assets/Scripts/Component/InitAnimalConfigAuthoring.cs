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
            int initAnimalNum = PlayerPrefs.GetInt("InitAnimalNum", authoring.initAnimalNumber);
            PlayerPrefs.SetInt("InitAnimalNum", initAnimalNum);
            float huntThreshold = PlayerPrefs.GetFloat("HuntThreshold", 0.7f);
            PlayerPrefs.SetFloat("HuntThreshold", huntThreshold);



            // Debug.Log("InitAnimalNum: " + initAnimalNum);

            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitAnimalConfig
            {
                animalPrefab = GetEntity(authoring.animalPrefab, TransformUsageFlags.Dynamic),
                initAnimalNumber = initAnimalNum,
                fieldSize = authoring.fieldSize,
                minInitSize = authoring.minInitSize,
                maxInitSize = authoring.maxInitSize,
                huntThreshold = huntThreshold, // for comparing size
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

    public float huntThreshold; // for comparing size
}
