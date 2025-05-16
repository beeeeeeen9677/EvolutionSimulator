using Unity.Entities;
using UnityEngine;

public class GrassTag_GreenAuthoring : MonoBehaviour
{


    public class GreenTagBaker : Baker<GrassTag_GreenAuthoring>
    {
        public override void Bake(GrassTag_GreenAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GrassTag_Green());
        }
    }
}

public struct GrassTag_Green : IComponentData
{

}