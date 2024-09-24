using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnCubeConfigAuthoring : MonoBehaviour
{


    public GameObject cubePrefab;
    public int amountToSpawn;


    public class Baker : Baker<SpawnCubeConfigAuthoring>
    {
        public override void Bake(SpawnCubeConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);//no need transform

            AddComponent(entity, new SpawnCubeConfig
            {
                cubePrefabEntity = GetEntity(authoring.cubePrefab, TransformUsageFlags.Dynamic),//Get Entity from prefab GameObject
                amountToSpawn = authoring.amountToSpawn,
            });
        }
    }
}

public struct SpawnCubeConfig : IComponentData
{
    public Entity cubePrefabEntity;
    public int amountToSpawn;
}
