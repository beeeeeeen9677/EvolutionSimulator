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


    public void GrowUp(float deltaTime) 
    {
        // eatable
        if (currentSize >= maxSize)
        {
            activated = true;
            return;
        }

        currentSize += deltaTime * 0.1f;
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
