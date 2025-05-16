using Unity.Entities;
using UnityEngine;

public class CuriosityAuthoring : MonoBehaviour
{


    public class CuriousityAuthoringBaker : Baker<CuriosityAuthoring>
    {
        public override void Bake(CuriosityAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Curiosity());
        }
    }
}



public struct Curiosity : IComponentData
{
    public float probability; // probability of taking curiousity action, between 0 - 1
    public float remainTime;
    public float maxTime; // max time for taking curiousity action
}