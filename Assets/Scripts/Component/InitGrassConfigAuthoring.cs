using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitGrassConfigAuthoring : MonoBehaviour
{
    public GameObject grassPrefab_Green;
    public GameObject grassPrefab_Orange;
    public GameObject grassPrefab_Purple;
    public GameObject grassPrefab_Pink;


    public int initGrassNumber;


    public class InitGrassConfigBaker : Baker<InitGrassConfigAuthoring>
    {
        public override void Bake(InitGrassConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitGrassConfig
            {
                grassPrefab = GetEntity(authoring.grassPrefab_Green, TransformUsageFlags.None),
                initGrassNumber = authoring.initGrassNumber,
            });


            DynamicBuffer<GrassPrefabElement> buffer = AddBuffer<GrassPrefabElement>(entity);
            buffer.Add(new GrassPrefabElement { grassPrefabs = GetEntity(authoring.grassPrefab_Green, TransformUsageFlags.None) });
            buffer.Add(new GrassPrefabElement { grassPrefabs = GetEntity(authoring.grassPrefab_Orange, TransformUsageFlags.None) });
            buffer.Add(new GrassPrefabElement { grassPrefabs = GetEntity(authoring.grassPrefab_Purple, TransformUsageFlags.None) });
            buffer.Add(new GrassPrefabElement { grassPrefabs = GetEntity(authoring.grassPrefab_Pink, TransformUsageFlags.None) });

        }
    }
}


public struct InitGrassConfig : IComponentData
{
    public Entity grassPrefab;
    public int initGrassNumber; // number to spawn initially
}

//Dynamic Buffer for storing different types of grasses
public struct GrassPrefabElement : IBufferElementData
{
    public Entity grassPrefabs;
}