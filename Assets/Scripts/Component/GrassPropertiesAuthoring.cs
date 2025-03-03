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



    public class GrassPropertiesBaker : Baker<GrassPropertiesAuthoring>
    {
        public override void Bake(GrassPropertiesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GrassProperties
            {
                currentSize = authoring.currentSize,
                maxSize = authoring.maxSize,
                provideEnergy = authoring.provideEnergy,
                activated = false,
                grid_Moisture = 0,
                grid_Nutrient = 0
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
}