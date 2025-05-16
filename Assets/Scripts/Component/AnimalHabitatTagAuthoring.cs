using Unity.Entities;
using UnityEngine;

public class AnimalHabitatTagAuthoring : MonoBehaviour
{


    public class IsFindingHabitatTagBaker : Baker<AnimalHabitatTagAuthoring>
    {
        public override void Bake(AnimalHabitatTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new IsFindingHabitat());
            SetComponentEnabled<IsFindingHabitat>(entity, false);

            AddComponent(entity, new AnimalHabitatInfo());
        }
    }
}


// Attach to animal
// Enable after finish eating successfully
// When enabled, handle by find habitat system to update/find a new habitat
public struct IsFindingHabitat : IComponentData, IEnableableComponent
{

}

// Store the habitat information for animal
public struct AnimalHabitatInfo : IComponentData
{
    public HabitatProperty? habitatProperty;
    public Vector3 habitatPosition;
}