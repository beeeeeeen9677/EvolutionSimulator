using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CuriousityAuthoring : MonoBehaviour
{


    public class CuriousityAuthoringBaker : Baker<CuriousityAuthoring>
    {
        public override void Bake(CuriousityAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Curiousity());
        }
    }
}



public struct Curiousity : IComponentData
{
    public float probability; // probability of taking curiousity action, between 0 - 1
    public float remainTime;
    public float maxTime; // max time for taking curiousity action
}