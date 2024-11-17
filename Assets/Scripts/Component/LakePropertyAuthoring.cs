using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class LakePropertyAuthoring : MonoBehaviour
{
    public float lakeRange;
    public int numberOfGrass;


    public class LakeBaker : Baker<LakePropertyAuthoring>
    {
        public override void Bake(LakePropertyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new LakeProperty
            {
                size = 1,
                lakeRange = authoring.lakeRange,
                numberOfGrass = authoring.numberOfGrass,
            });
        }
    }
}

public struct LakeProperty : IComponentData
{
    public float size;
    public float lakeRange;
    public int numberOfGrass;
}