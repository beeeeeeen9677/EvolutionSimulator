using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct InitAnimalConfigSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitAnimalConfig>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }


    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only

        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>(); 



        for(int i = 0; i< initAnimalConfig.initAnimalNumber; i++)
        {
            Entity newSpawnedAnimal = state.EntityManager.Instantiate(initAnimalConfig.animalPrefab);


            SystemAPI.SetComponent(newSpawnedAnimal, new LocalTransform
            {
                Position = new float3(UnityEngine.Random.Range(-50, 50), 0.5f, UnityEngine.Random.Range(-50, 50)),
                Rotation = quaternion.identity,
                Scale = 1
            });


            // assign random value to speed
            SystemAPI.SetComponent(newSpawnedAnimal, new Movement
            {
                speed = UnityEngine.Random.Range(1, 10)
            });
        }
    }
}
