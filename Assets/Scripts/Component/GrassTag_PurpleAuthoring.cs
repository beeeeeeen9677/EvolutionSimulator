using Unity.Entities;
using UnityEngine;

public class GrassTag_PurpleAuthoring : MonoBehaviour
{


    public class PurpleTagBaker : Baker<GrassTag_PurpleAuthoring>
    {
        public override void Bake(GrassTag_PurpleAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GrassTag_Purple());
        }
    }
}

public struct GrassTag_Purple : IComponentData
{

}