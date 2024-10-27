using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TargetAuthoring : MonoBehaviour
{
    public class TargetBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target
            {
                maxChaseTime = 15f,
                remainChaseTime = 0f
            });
        }
    }
}

public struct Target : IComponentData
{
    public Entity targetEntity;
    public float3 targetPosition;
    public float maxChaseTime; // max time for chasing target
    public float remainChaseTime;
}
