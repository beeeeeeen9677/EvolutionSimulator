using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AnimalTagsAuthoring : MonoBehaviour
{


    public class AnimalTagBaker : Baker<AnimalTagsAuthoring>
    {
        public override void Bake(AnimalTagsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimalTag());

            AddComponent(entity, new SensorTriggerTag());
            SetComponentEnabled<SensorTriggerTag>(entity, false);

            AddComponent(entity, new ReproductionTag());
            SetComponentEnabled<ReproductionTag>(entity, false);
        }
    }
}

public struct AnimalTag : IComponentData
{
}
public struct SensorTriggerTag: IComponentData, IEnableableComponent
{
}
public struct ReproductionTag : IComponentData, IEnableableComponent
{
}