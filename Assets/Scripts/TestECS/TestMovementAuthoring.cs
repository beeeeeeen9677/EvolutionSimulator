using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TestMovementAuthoring : MonoBehaviour
{


    public class Baker : Baker<TestMovementAuthoring>
    {
        public override void Bake(TestMovementAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TestMovement
            {
                //this Random only happen to prefab once, so all Entity with this compoenent have same value
                movementVector = new float3(UnityEngine.Random.Range(-1, 1), 0, UnityEngine.Random.Range(-1, 1)),
            });
        }
    }
}

public struct TestMovement : IComponentData
{
    public float3 movementVector;
}
