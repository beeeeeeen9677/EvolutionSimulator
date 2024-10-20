using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AnimalTagAuthoring : MonoBehaviour
{


    public class AnimalTagBaker : Baker<AnimalTagAuthoring>
    {
        public override void Bake(AnimalTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimalTag());
        }
    }
}

public struct AnimalTag : IComponentData
{

}