using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class HungerStatusAuthoring : MonoBehaviour
{
    public float hungryThreshold; // threshold of max energy (0 to 1)



    public class HungerStatusBaker : Baker<HungerStatusAuthoring>
    {
        public override void Bake(HungerStatusAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HungerStatus
            {
                hungryThreshold = authoring.hungryThreshold,
                currentState = HungerStatusEnum.full,
            });
        }
    }

}


public struct HungerStatus : IComponentData
{
    public float hungryThreshold; // threshold of max energy (0 to 1)

    public HungerStatusEnum currentState;
}

public enum HungerStatusEnum
{
    hungry,
    full
}