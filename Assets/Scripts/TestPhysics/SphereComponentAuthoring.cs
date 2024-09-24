using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SphereComponentAuthoring : MonoBehaviour
{

    public float moveSpeed = 10;

    public class SphereComponentBaker : Baker<SphereComponentAuthoring>
    {
        public override void Bake(SphereComponentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SphereComponent
            {
                moveSpeed = authoring.moveSpeed,
            });
        }
    }
}

public struct SphereComponent : IComponentData
{
    public float moveSpeed;
}