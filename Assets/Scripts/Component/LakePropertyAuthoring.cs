using Unity.Entities;
using UnityEngine;

public class LakePropertyAuthoring : MonoBehaviour
{
    public int lakeRange; // number of grids
    public int numberOfGrass;
    public int initSoilMoisture; // multiple of 2 (0 - 16)



    public class LakeBaker : Baker<LakePropertyAuthoring>
    {
        public override void Bake(LakePropertyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new LakeProperty
            {
                size = 1,
                lakeRange = authoring.lakeRange,
                numberOfGrass = authoring.numberOfGrass,
                initSoilMoisture = authoring.initSoilMoisture,
            });
        }
    }
}

public struct LakeProperty : IComponentData
{
    public float size;
    public int lakeRange; // number of grids
    public int numberOfGrass;
    public int initSoilMoisture; // multiple of 2 (0 - 16)

}