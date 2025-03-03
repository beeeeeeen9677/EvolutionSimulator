using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


// Deprecated
public partial struct InitGrassConfigSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitGrassConfig>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }


    // Deprecated
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only

        InitGrassConfig initGrassConfig = SystemAPI.GetSingleton<InitGrassConfig>();
        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();

        Entity initGrassConfigEntity = SystemAPI.GetSingletonEntity<InitGrassConfig>();



        int fieldSize = initAnimalConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }


        for(int i = 0; i < initGrassConfig.initGrassNumber; i++)
        {
            // get random type of grasses
            var buffer = state.EntityManager.GetBuffer<GrassPrefabElement>(initGrassConfigEntity);
            int grassIndex = UnityEngine.Random.Range(0, buffer.Length);
            Entity newSpawnedGrass = state.EntityManager.Instantiate(buffer[grassIndex].grassPrefabs);


            //Entity newSpawnedGrass = state.EntityManager.Instantiate(initGrassConfig.grassPrefab); //old code

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

