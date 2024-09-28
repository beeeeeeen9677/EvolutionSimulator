using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TestPhySphereAuthoring : MonoBehaviour
{
    public class TestPhySphereBaker : Baker<TestPhySphereAuthoring>
    {
        public override void Bake(TestPhySphereAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TestPhySphere
            {
            });
        }
    }
}

public struct TestPhySphere : IComponentData
{
    public float moveSpeed;
}

