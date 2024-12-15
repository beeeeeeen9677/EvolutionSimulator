using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SprintAuthoring : MonoBehaviour
{


    public class SprintBaker : Baker<SprintAuthoring>
    {
        public override void Bake(SprintAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Sprint
            {
                effectCounter = 0,
                effectTotalTime = 5,

                //cooldownCounter = 0,
                //cooldownInterval = 0,
            });
        }
    }
}


public struct Sprint : IComponentData
{
    public float effectCounter;
    public float effectTotalTime;

    //public float cooldownCounter;
    //public float cooldownInterval;
}
