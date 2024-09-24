using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial class SpawnCubeSystem : SystemBase//can use ISystem
{
    protected override void OnCreate()
    {
        this.Enabled = false;

        RequireForUpdate<SpawnCubeConfig>();
    }
    protected override void OnUpdate()
    {
        //run this code once only
        this.Enabled = false;

        //Singleton: one and only one entity with this component
        SpawnCubeConfig spawnCubeConfig = SystemAPI.GetSingleton<SpawnCubeConfig>();


        //or put them in a Native Array then Spawn/Instantiate once
        for (int i = 0; i < spawnCubeConfig.amountToSpawn; i++) 
        {
            //only can use in for loop only
            Entity spawnedEntity = EntityManager.Instantiate(spawnCubeConfig.cubePrefabEntity);


            //EntityManager.SetComponentData or SystemAPI.SetComponent (better)
            SystemAPI.SetComponent(spawnedEntity, new LocalTransform
            {
                Position = new float3(UnityEngine.Random.Range(-10, 5), 0.5f, UnityEngine.Random.Range(-4, 7)),
                Rotation = quaternion.identity,
                Scale = 1f,//MUST add (default scale is 0)
            });

            // EntityManager.AddComponent(...)
            // avoid adding/removing component to Entity since it is structial change/creating archetype,
            // better to ENABLE component
        }
    }
}
