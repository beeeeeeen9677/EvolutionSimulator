using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

public class TestTriggerComponentAuthoring : MonoBehaviour
{
    public float size;

    public class TriggerComponentBaker : Baker<TestTriggerComponentAuthoring>
    {
        public override void Bake(TestTriggerComponentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TestTriggerComponent
            {
                size = authoring.size,
            });
        }
    }
}

public struct TestTriggerComponent : IComponentData
{
    public float size;
}
