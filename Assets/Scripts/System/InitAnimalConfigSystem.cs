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

        int fieldSize = initAnimalConfig.fieldSize;

        // avoid invalid input
        if(fieldSize <= 0)
        {
            fieldSize = 50;
        }


        for(int i = 0; i< initAnimalConfig.initAnimalNumber; i++)
        {
            Entity newSpawnedAnimal = state.EntityManager.Instantiate(initAnimalConfig.animalPrefab);



            float animalInitSize = UnityEngine.Random.Range(0.5f, 1f);
            SystemAPI.SetComponent(newSpawnedAnimal, new SizeProperty
            {
                initSize = animalInitSize,
                maxSize = animalInitSize * 1.5f,
            });



            SystemAPI.SetComponent(newSpawnedAnimal, new LocalTransform
            {
                Position = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 1f, UnityEngine.Random.Range(-fieldSize, fieldSize)),
                Rotation = quaternion.identity,
                Scale = animalInitSize
            });


            // assign random value to speed
            SystemAPI.SetComponent(newSpawnedAnimal, new Movement
            {
                speed = UnityEngine.Random.Range(1, 6)
            });


            float maxEnergy = UnityEngine.Random.Range(80, 120);
            SystemAPI.SetComponent(newSpawnedAnimal, new Energy
            {
                maxEnergy = maxEnergy,
                currentEnergy = maxEnergy
            });


            float grassSensorProbability = UnityEngine.Random.Range(0f, 1f);
            SystemAPI.SetComponent(newSpawnedAnimal, new AnimalSensor
            {
                size = 500f,

                maxCooldown = 3,
                currentCooldown = 0,

                grassSensorProb = grassSensorProbability,
                animalSensorProb = 1 - grassSensorProbability,
            });
            

        }
    }
}
