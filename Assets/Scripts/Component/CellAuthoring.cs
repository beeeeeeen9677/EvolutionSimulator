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
    // normal cell
    public int numberOfNormalCell;
    // eat animal
    public int numberOfMeatCell;
    // eat grass
    public int numberOfGreenCell;
    public int numberOfOrangeCell;
    public int numberOfPurpleCell;
    public int numberOfPinkCell;
}


public enum ColorCell
{
    Green,
    Orange,
    Purple,
    Pink
}