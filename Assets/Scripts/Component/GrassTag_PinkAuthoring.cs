using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GrassTag_PinkAuthoring : MonoBehaviour
{


    public class PinkTagBaker : Baker<GrassTag_PinkAuthoring>
    {
        public override void Bake(GrassTag_PinkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GrassTag_Pink());
        }
    }
}

public struct GrassTag_Pink : IComponentData
{

}