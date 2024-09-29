using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InitGrassConfigAuthoring : MonoBehaviour
{
    public GameObject grassPrefab;
    public int initGrassNumber;


    public class InitGrassConfigBaker : Baker<InitGrassConfigAuthoring>
    {
        public override void Bake(InitGrassConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitGrassConfig
            {
                grassPrefab = GetEntity(authoring.grassPrefab, TransformUsageFlags.None),
                initGrassNumber = authoring.initGrassNumber,
            });
        }
    }
}


public struct InitGrassConfig : IComponentData
{
    public Entity grassPrefab;
    public int initGrassNumber; // number to spawn initially
}