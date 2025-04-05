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
        state.RequireForUpdate<InitGridSystemConfig>();
    }



    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only


        InitGridSystemConfig initGridSystemConfig = SystemAPI.GetSingleton<InitGridSystemConfig>();
        Entity initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
        var gridCellBuffer = state.EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
        int gridBufferWidth = initGridSystemConfig.width;
        int gridBufferHeight = initGridSystemConfig.height;
        int gridCellSize = initGridSystemConfig.gridCellSize;
        Vector3 gridSystemOrigin = initGridSystemConfig.originPosition;




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


            //float animalInitSize = UnityEngine.Random.Range(0.5f, 1f);
            float animalInitSize = UnityEngine.Random.Range(initAnimalConfig.minInitSize, initAnimalConfig.maxInitSize);
            SystemAPI.SetComponent(newSpawnedAnimal, new SizeProperty
            {
                initSize = animalInitSize,
                maxSize = animalInitSize * 1.5f,
            });



            // get the position of a random grid cell
            int randX = UnityEngine.Random.Range(0, gridBufferWidth);
            int randY = UnityEngine.Random.Range(0, gridBufferHeight);
            Vector3 newRandomPosition = GridBufferUtils.GetWorldPosition(randX, randY, gridCellSize, gridSystemOrigin);


            SystemAPI.SetComponent(newSpawnedAnimal, new LocalTransform
            {
                //Position = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 1f, UnityEngine.Random.Range(-fieldSize, fieldSize)),
                Position = newRandomPosition,
                Rotation = quaternion.identity,
                Scale = animalInitSize
            });


            // assign random value to speed
            SystemAPI.SetComponent(newSpawnedAnimal, new Movement
            {
                speed = UnityEngine.Random.Range(1, 6)
            });


            float maxEnergy = UnityEngine.Random.Range(80, 120);
            //maxEnergy = UnityEngine.Random.Range(18, 29);    // for test
            SystemAPI.SetComponent(newSpawnedAnimal, new Energy
            {
                maxEnergy = maxEnergy,
                currentEnergy = maxEnergy * 0.8f,
            });


            float grassSensorProbability = UnityEngine.Random.Range(0f, 1f);
            SystemAPI.SetComponent(newSpawnedAnimal, new AnimalSensor
            {
                size = 20f,

                warningRange = 10f,

                maxCooldown = 6,
                currentCooldown = 3,

                grassSensorProbability = grassSensorProbability,
                animalSensorProbability = 1 - grassSensorProbability,

                currentSensor = null,

                // 1 to 9
                greenWeight = UnityEngine.Random.Range(1, 10),
                orangeWeight = UnityEngine.Random.Range(1, 10),
                purpleWeight = UnityEngine.Random.Range(1, 10),
                pinkWeight = UnityEngine.Random.Range(1, 10),
            });



            SystemAPI.SetComponent(newSpawnedAnimal, new ReproductionCounter
            {
                interval = 10f,
                currentCD = 10f,
            });


            RefRW<Curiosity> curiousity = SystemAPI.GetComponentRW<Curiosity>(newSpawnedAnimal);
            curiousity.ValueRW.probability = 0.5f;
            curiousity.ValueRW.remainTime = 10;
            curiousity.ValueRW.maxTime = 10;
        }
    }
}
