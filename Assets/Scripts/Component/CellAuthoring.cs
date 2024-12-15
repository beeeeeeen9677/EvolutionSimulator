using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CellAuthoring : MonoBehaviour
{


    public class CellBaker : Baker<CellAuthoring>
    {
        public override void Bake(CellAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Cell
            {
                numberOfNormalCell = 0,
                numberOfMeatCell = 0,
            });
        }
    }

}

public struct Cell : IComponentData
{
    public int numberOfNormalCell;
    public int numberOfMeatCell;
}