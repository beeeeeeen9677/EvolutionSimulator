using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct GrassAspect : IAspect
{
    public readonly Entity entity;
    public readonly RefRW<LocalTransform> _localTransform;
    public readonly RefRW<GrassProperties> _grassProperties;

    private float maxSize => _grassProperties.ValueRO.maxSize;

    public float currentSize
    {
        get => _grassProperties.ValueRO.currentSize;
        private set => _grassProperties.ValueRW.currentSize = value;
    }

    public float provideEnergy
    {
        get => _grassProperties.ValueRO.provideEnergy;
        private set => _grassProperties.ValueRW.provideEnergy = value;
    }

    public bool activated
    {
        get => _grassProperties.ValueRO.activated;
        private set => _grassProperties.ValueRW.activated = value;
    }

    public float grid_Moisture
    {
        get => _grassProperties.ValueRO.grid_Moisture;
        set => _grassProperties.ValueRW.grid_Moisture = value;
    }

    public float grid_Nutrient
    {
        get => _grassProperties.ValueRO.grid_Nutrient;
        set => _grassProperties.ValueRW.grid_Nutrient = value;
    }

    public float growthRate => _grassProperties.ValueRO.GetGrowthRate();


    public float growthRate_Nutrient => _grassProperties.ValueRO.growthRate_Nutrient;
    public float growthRate_Moisture => _grassProperties.ValueRO.growthRate_Moisture;


    public void GrowUp(float deltaTime) 
    {
        // eatable
        if (currentSize >= maxSize)
        {
            activated = true;
            return;
        }

        //float temp = deltaTime * (0.02f + grid_Moisture * 0.2f + grid_Nutrient * 0.5f);
        //currentSize += temp;
        //Debug.Log("currentSize: " + temp);

        currentSize += deltaTime * growthRate;




        currentSize = Mathf.Clamp(currentSize, 0, maxSize);

        _localTransform.ValueRW.Scale = currentSize;


        //Debug.Log("GrassSize: " + currentSize);
    }



    // call when this grass was eaten by others
    public void WasEaten()
    {
        activated = false;
        currentSize = 0;
    }
}
