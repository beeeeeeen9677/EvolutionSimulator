using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class HabitatPropertyAuthoring : MonoBehaviour
{
    public class HabitatPropertyBaker : Baker<HabitatPropertyAuthoring>
    {
        public override void Bake(HabitatPropertyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new HabitatProperty());
        }
    }
}


public struct HabitatProperty : IComponentData
{
    // the MINIMUM & MAXIMUM value of max size of the animal living here (Limitation)
    public float minSize;
    public float maxSize;
    public float radius; // area of the habitat
    public float effectOfHiding; // a number between 0 - 1 to decide whether can be detected by enemy, larger better
}