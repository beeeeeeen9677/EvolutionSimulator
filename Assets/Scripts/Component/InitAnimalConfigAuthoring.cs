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
            float maxInitSize = PlayerPrefs.GetFloat("MaxInitSize", authoring.maxInitSize);
            PlayerPrefs.SetFloat("MaxInitSize", maxInitSize);
            float compoundHuntThresholdRate = PlayerPrefs.GetFloat("CompoundHuntThresholdRate", 0.05f);
            PlayerPrefs.SetFloat("CompoundHuntThresholdRate", compoundHuntThresholdRate);

            int adultAgeError = PlayerPrefs.GetInt("AdultAgeError", 3);
            PlayerPrefs.SetInt("AdultAgeError", adultAgeError);
            int adultDuration = PlayerPrefs.GetInt("AdultDuration", 45);
            PlayerPrefs.SetInt("AdultDuration", adultDuration);
            float agingError = PlayerPrefs.GetFloat("AgingError", 0.25f);
            PlayerPrefs.SetFloat("AgingError", agingError);


            float reproductionCost = PlayerPrefs.GetFloat("ReproductionCost", 0.2f);
            PlayerPrefs.SetFloat("ReproductionCost", reproductionCost);



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
                compoundHuntThresholdRate = compoundHuntThresholdRate,
                adultAgeError = adultAgeError,
                adultDuration = adultDuration,
                agingError = agingError,
                reproductionCost = reproductionCost,
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
    public float compoundHuntThresholdRate; // use with hunt threshold

    public int adultAgeError; // error of age when animal is adult, affected by food preference
    public int adultDuration; // duration of adult age, affected by food preference
    public float agingError; // (0 - 1) error of age when animal is aging, affected by food preference

    public float reproductionCost; // cost of reproduction
}
