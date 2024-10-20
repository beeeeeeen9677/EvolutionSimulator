using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public class SensorAuthoring : MonoBehaviour
{
    //public float size;

    public class SensorbBaker : Baker<SensorAuthoring>
    {
        public override void Bake(SensorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimalSensor
            {
                //size = authoring.size,
            });

        }
    }
}


// for trigger detection
public struct AnimalSensor : IComponentData
{
    public float size; // radius

    public float maxCooldown; // CD of the sensor
    public float currentCooldown;

    // all sensor probability sum should be 1
    public float grassSensorProb;
    public float animalSensorProb;
    //public float deadbodySensorProb;
}