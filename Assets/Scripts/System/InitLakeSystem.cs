using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct InitLakeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitLakeConfig>();
        state.RequireForUpdate<InitGrassConfig>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only

        InitLakeConfig initLakeConfig = SystemAPI.GetSingleton<InitLakeConfig>();

        InitGrassConfig initGrassConfig = SystemAPI.GetSingleton<InitGrassConfig>();



        int fieldSize = initLakeConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }

        // spawn lake
        for (int i = 0; i < initLakeConfig.initLakeNumber; i++)
        {
            Entity newSpawnedLake = state.EntityManager.Instantiate(initLakeConfig.lakePrefab);

            LakeProperty lakeProperty = SystemAPI.GetComponent<LakeProperty>(newSpawnedLake);



            float3 lakePosition = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0, UnityEngine.Random.Range(-fieldSize, fieldSize));

            SystemAPI.SetComponent(newSpawnedLake, new LocalTransform
            {
                Position = lakePosition,
                Rotation = quaternion.identity,
                Scale = 1
            });




            // spawn grass for current lake
            int grassNumOfCurrentLake = lakeProperty.numberOfGrass;
            for (int j = 0; j < grassNumOfCurrentLake; j++)
            {
                Entity newSpawnedGrass = state.EntityManager.Instantiate(initGrassConfig.grassPrefab);


                SystemAPI.SetComponent(newSpawnedGrass, new LocalTransform
                {
                    Position = new 
                        float3(UnityEngine.Random.Range(-lakeProperty.lakeRange, lakeProperty.lakeRange), 
                        0.5f, 
                        UnityEngine.Random.Range(-lakeProperty.lakeRange, lakeProperty.lakeRange)) 
                        + lakePosition,
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
}

