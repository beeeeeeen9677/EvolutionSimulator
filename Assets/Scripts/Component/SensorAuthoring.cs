using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public class SensorAuthoring : MonoBehaviour
{
    public float size;

    public class SensorbBaker : Baker<SensorAuthoring>
    {
        public override void Bake(SensorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimalSensor
            {
                size = authoring.size,
            });

        }
    }
}


// for trigger detection
public struct AnimalSensor : IComponentData
{
    public float size; //radius
}