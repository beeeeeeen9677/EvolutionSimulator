using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SelectableUnitTagAuthoring : MonoBehaviour
{


    public class SelectableUnitTagBaker : Baker<SelectableUnitTagAuthoring>
    {
        public override void Bake(SelectableUnitTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SelectableUnitTag());

            AddComponent(entity, new IsSelectedTag());
            SetComponentEnabled<IsSelectedTag>(entity, false); // default not selected
        }
    }
}

// this entity can be selected
public struct SelectableUnitTag : IComponentData
{

}

// use with SelectableUnitTag
public struct IsSelectedTag : IComponentData, IEnableableComponent
{

}
