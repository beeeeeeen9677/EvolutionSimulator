using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MovementAuthoring : MonoBehaviour
{
   // public float speed;

    public class MovementBaker : Baker<MovementAuthoring>
    {
        public override void Bake(MovementAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Movement
            {
                //speed = authoring.speed,
            });
        }
    }
}

public struct Movement : IComponentData
{
    public float speed;
}