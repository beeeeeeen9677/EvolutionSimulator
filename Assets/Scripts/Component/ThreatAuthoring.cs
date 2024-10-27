using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ThreatAuthoring : MonoBehaviour
{


    public class ThreatBaker : Baker<ThreatAuthoring>
    {
        public override void Bake(ThreatAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Threat());
        }
    }
}


public struct Threat : IComponentData
{
    public Entity threatEntity;
    public float3 threatPosition;
}