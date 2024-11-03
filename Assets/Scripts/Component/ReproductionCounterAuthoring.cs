using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ReproductionCounterAuthoring : MonoBehaviour
{


    public class ReproductionCounterBaker : Baker<ReproductionCounterAuthoring>
    {
        public override void Bake(ReproductionCounterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ReproductionCounter());
        }
    }
}



public struct ReproductionCounter : IComponentData
{
    public float interval; // max time for chasing target
    public float currentCD;
}