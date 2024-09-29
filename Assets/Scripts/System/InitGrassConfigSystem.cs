using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct InitGrassConfigSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitGrassConfig>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only

        InitGrassConfig initGrassConfig = SystemAPI.GetSingleton<InitGrassConfig>();
        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();


        int fieldSize = initAnimalConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }


        for(int i = 0; i < initGrassConfig.initGrassNumber; i++)
        {
            Entity newSpawnedGrass = state.EntityManager.Instantiate(initGrassConfig.grassPrefab);

            SystemAPI.SetComponent(newSpawnedGrass, new LocalTransform
            {
                Position = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0.5f, UnityEngine.Random.Range(-fieldSize, fieldSize)),
                Rotation = quaternion.identity,
                Scale = 1
            });


            // set in inspector
            /*
            SystemAPI.SetComponent(newSpawnedGrass, new GrassProperties
            {
                currentSize = 1,
                maxSize = 1,
            });
            */
        }
    }
}

