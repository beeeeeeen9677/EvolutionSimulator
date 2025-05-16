using Unity.Entities;
using UnityEngine;

public class EnergyAuthoring : MonoBehaviour
{
    //public float currentEnergy;
    //public float maxEnergy;


    public class EnergyBaker : Baker<EnergyAuthoring>
    {
        public override void Bake(EnergyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Energy
            {
                //currentEnergy = authoring.currentEnergy,
                //maxEnergy = authoring.maxEnergy,
            });
        }
    }
}

public struct Energy : IComponentData
{
    public float currentEnergy;
    public float maxEnergy;
}
