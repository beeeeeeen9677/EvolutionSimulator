using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct InitHabitatSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitHabitatConfig>();
    }


    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; // run for one loop only


        InitLakeConfig initLakeConfig = SystemAPI.GetSingleton<InitLakeConfig>();
        int fieldSize = initLakeConfig.fieldSize;

        // avoid invalid input
        if (fieldSize <= 0)
        {
            fieldSize = 50;
        }



        // for setting MAX & MIN size value
        InitAnimalConfig initAnimalConfig = SystemAPI.GetSingleton<InitAnimalConfig>();
        float minInitValue = initAnimalConfig.minInitSize * 1.5f;
        float maxInitValue = initAnimalConfig.maxInitSize * 1.5f;
        Debug.Log($"InitAnimalConfig   min: {minInitValue}  max: {maxInitValue}");

        InitHabitatConfig initHabitatConfig = SystemAPI.GetSingleton<InitHabitatConfig>();

        float maxRadius = initHabitatConfig.maxRadius; // 30
        float minRadius = initHabitatConfig.minRadius; // 15
        float effectOfHiding = initHabitatConfig.effectOfHiding;


        for (int i = 0; i < initHabitatConfig.numberOfHabitat; i++)
        {
            Entity newSpawnedHabitat = state.EntityManager.Instantiate(initHabitatConfig.habitatPrefab);


            // random position
            float3 habitatPosition = new float3(UnityEngine.Random.Range(-fieldSize, fieldSize), 0, UnityEngine.Random.Range(-fieldSize, fieldSize));
            SystemAPI.SetComponent(newSpawnedHabitat, new LocalTransform
            {
                Position = habitatPosition,
                Rotation = quaternion.identity,
                Scale = 1
            });


            // set MAX & MIN size value
            float midValue = UnityEngine.Random.Range(minInitValue, maxInitValue);
            float minSize = midValue - initHabitatConfig.interval / 2;
            float maxSize = midValue + initHabitatConfig.interval / 2;

            float radius = UnityEngine.Random.Range(minRadius, maxRadius);
            SystemAPI.SetComponent(newSpawnedHabitat, new HabitatProperty
            {
                minSize = minSize,
                maxSize = maxSize,
                radius = radius,
                effectOfHiding = effectOfHiding,
            });


            HabitatProperty habitatProperty = SystemAPI.GetComponent<HabitatProperty>(newSpawnedHabitat);
            /*
            Debug.Log($"mid: {midValue} interval: {initHabitatConfig.interval / 2}    min: {minSize}    max: {maxSize}\n" +
                $"Habitat {i} - min: {habitatProperty.minSize}    max: {habitatProperty.maxSize}");
            */
            //Debug.Log($"Habitat {i}:  min: {habitatProperty.minSize}    max: {habitatProperty.maxSize}");




            // create domain effect by radius
            Entity newDomainEffect = state.EntityManager.Instantiate(initHabitatConfig.domainEffectPrefab);
            SystemAPI.SetComponent(newDomainEffect, new LocalTransform
            {
                Position = habitatPosition,
                Rotation = quaternion.identity,
                Scale = radius * 2,
            });
        }
    }
}

