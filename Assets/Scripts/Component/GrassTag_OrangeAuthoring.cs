using Unity.Entities;
using UnityEngine;

public class GrassTag_OrangeAuthoring : MonoBehaviour
{


    public class OrangeTagBaker : Baker<GrassTag_OrangeAuthoring>
    {
        public override void Bake(GrassTag_OrangeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GrassTag_Orange());
        }
    }
}

public struct GrassTag_Orange : IComponentData
{

}