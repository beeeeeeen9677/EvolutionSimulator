using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SizePropertyAuthoring : MonoBehaviour
{


    public class SizePropertyBaker : Baker<SizePropertyAuthoring>
    {
        public override void Bake(SizePropertyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SizeProperty());
        }
    }
}

public struct SizeProperty : IComponentData
{
    public float currentSize;
    public float initSize;
    public float maxSize;
}