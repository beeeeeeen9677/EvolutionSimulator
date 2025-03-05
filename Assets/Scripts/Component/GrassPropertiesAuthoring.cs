using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GrassPropertiesAuthoring : MonoBehaviour
{
    public float currentSize;
    public float maxSize;
    public float provideEnergy;

    [Header("Growth Rate Param")]
    public float baseGrowthRate;

    [Header("Growth Rate - Moisture")]
    public float lowerLimit_Moisture;
    public float upperLimit_Moisture;
    public float growthRate_Moisture;

    [Header("Growth Rate - Nutrient")]
    public float lowerLimit_Nutrient;
    public float upperLimit_Nutrient;
    public float growthRate_Nutrient;


    public class GrassPropertiesBaker : Baker<GrassPropertiesAuthoring>
    {
        public override void Bake(GrassPropertiesAuthoring authoring)
        {

            // growth rate params related
            float growth_A_Moisture = (authoring.lowerLimit_Moisture + authoring.upperLimit_Moisture) / 2;
            float growth_K_Moisture = 10 / Mathf.Pow((authoring.lowerLimit_Moisture - growth_A_Moisture), 2); // k = B / (LowLim - A)^2

            float growth_A_Nutrient = (authoring.lowerLimit_Nutrient + authoring.upperLimit_Nutrient) / 2;
            float growth_K_Nutrient = 10 / Mathf.Pow((authoring.lowerLimit_Nutrient - growth_A_Nutrient), 2); // k = B / (LowLim - A)^2
            //Debug.Log("base: " + authoring.baseGrowthRate );


            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GrassProperties
            {
                currentSize = authoring.currentSize,
                maxSize = authoring.maxSize,
                provideEnergy = authoring.provideEnergy,
                activated = false,

                grid_Moisture = 0,
                grid_Nutrient = 0,

                baseGrowthRate = authoring.baseGrowthRate,

                lowerLimit_Moisture = authoring.lowerLimit_Moisture,
                upperLimit_Moisture = authoring.upperLimit_Moisture,
                growth_k_Moisture = growth_K_Moisture, // k = B / (LowLim - A)^2
                growthRate_Moisture = authoring.growthRate_Moisture,

                lowerLimit_Nutrient = authoring.lowerLimit_Nutrient,
                upperLimit_Nutrient = authoring.upperLimit_Nutrient,
                growth_k_Nutrient = growth_K_Nutrient, // k = B / (LowLim - A)^2
                growthRate_Nutrient = authoring.growthRate_Nutrient
            });
        }
    }
}


public struct GrassProperties : IComponentData
{
    public float currentSize;
    public float maxSize;
    public bool activated; // true: eatable
    public float provideEnergy; // how many energy can be obtained after eating


    // grid cell properties, set by GridUpdateSystem
    public float grid_Moisture;
    public float grid_Nutrient;


    // growth rate/effect params related to moisture and nutrient

    // Quadratic equation that:
    // 1. max y(result) should be at most 10
    // 2. if the given soil moisture is between the upper/lower limit, it can obtain 0 to 10 in this equation
    // Parameters:
    // y = -k (x - A)^2 + B
    // k = B / (LowLim - A)^2
    // A = (LowerLimit + Upper Limit) / 2
    // B = 10 (max output)
    // x (input) = grid_Mopisture OR grid_Nutrient
    public float baseGrowthRate;


    public float lowerLimit_Moisture;
    public float upperLimit_Moisture;
    public float growth_k_Moisture;
    public float growthRate_Moisture;


    public float lowerLimit_Nutrient;
    public float upperLimit_Nutrient;
    public float growth_k_Nutrient;
    public float growthRate_Nutrient;


    public float GetGrowthRate() 
    {


        // y = -k (x - A)^2 + B
        float moistureRate = -growth_k_Moisture * Mathf.Pow(grid_Moisture - (lowerLimit_Moisture + upperLimit_Moisture) / 2, 2) + 10;
        float moistureEffect = Mathf.Clamp(moistureRate, 0, 10) * 0.1f;

        float nutrientRate = -growth_k_Nutrient * Mathf.Pow(grid_Nutrient - (lowerLimit_Nutrient + upperLimit_Nutrient) / 2, 2) + 10;
        float nutrientEffect = Mathf.Clamp(nutrientRate, 0, 10) * 0.1f;

        float growthRate = (baseGrowthRate + moistureEffect * growthRate_Moisture + nutrientEffect * growthRate_Nutrient) * 0.1f;


        // e.g. currentSize += deltaTime * (0.02f + grid_Moisture * 0.2f + grid_Nutrient * 0.5f);

        //Debug.Log("base: " + baseGrowthRate + " moistureRate: " + moistureRate + "   nutrientRate: " + nutrientRate );

        /*
        Debug.Log("growthRate: " + growthRate + " base: " + baseGrowthRate + 
            " | moistureRate: " + moistureEffect + " moistureEffect: " + moistureEffect * growthRate_Moisture + 
            " | nutrientRate: " + nutrientEffect + " nutrientEffect: " + nutrientEffect * growthRate_Nutrient);
        */

        return growthRate;
    }

}