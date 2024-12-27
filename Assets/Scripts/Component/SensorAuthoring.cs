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

    public float warningRange;// max distance to detect predator

    public float maxCooldown; // CD of the sensor
    public float currentCooldown;

    // all sensor probability sum should be 1
    public float grassSensorProbability;
    public float animalSensorProbability;
    //public float deadbodySensorProb;

    public CollisionLayer? currentSensor; // the sensor using currently


    // weight of different grasses
    public int greenWeight;
    public int orangeWeight;
    public int purpleWeight;
    public int pinkWeight;

}