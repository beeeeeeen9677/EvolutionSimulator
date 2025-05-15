using System;
using Unity.Entities;
using UnityEngine;


public partial struct ConfigUpdateSystem : ISystem
{
    // public Action<int, int, int, DynamicBuffer<GridCell>> OnGridsInit;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitAnimalConfig>();
        state.RequireForUpdate<InitLakeConfig>();
        state.RequireForUpdate<InitHabitatConfig>();
        state.RequireForUpdate<InitGridSystemConfig>();
    }



    public void OnUpdate(ref SystemState state)
    {
        // Only need to run once at startup
        state.Enabled = false;


        Debug.Log("ConfigUpdateSystem: Start config");


        // First ensure all PlayerPrefs have default values if they're missing
        InitializeMissingPlayerPrefs(ref state);

        // Then update all config components
        UpdateConfigComponents(ref state);


        // Update Grid System
        UpdateGridSystem(ref state);

    }

    private void UpdateGridSystem(ref SystemState state)
    {
        foreach (var (config, buffer) in
                    SystemAPI.Query<RefRW<InitGridSystemConfig>, DynamicBuffer<GridCell>>())
        {
            int newSize = PlayerPrefs.GetInt("InitFieldSize", config.ValueRO.width);


            // Only initialize if:
            // 1. Buffer is completely empty (first run), OR
            // 2. Size has changed from config
            if (buffer.IsEmpty || newSize != config.ValueRO.width)
            {
                // Reinitialize the grid buffer
                buffer.Clear();

                // Resize the grid if field size changed
                config.ValueRW.width = newSize;
                config.ValueRW.height = newSize;
                config.ValueRW.remainingGrids = newSize * newSize;

                for (int x = 0; x < newSize; x++)
                {
                    for (int y = 0; y < newSize; y++)
                    {
                        buffer.Add(new GridCell
                        {
                            X = x,
                            Y = y,
                            storingObject = Entity.Null,
                            soilMoisture_value = 0,
                            soilNutrient = 0,
                            soilDensity = 1
                        });
                    }
                }


                /*
                Entity initGridSystemConfigEntity;
                Vector3 gridSystemOrigin;
                int gridBufferWidth;
                int gridCellSize;



                // grid system config var
                var initGridSystemConfig = config;
                initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
                DynamicBuffer<GridCell> gridCellBuffer = buffer;
                gridBufferWidth = config.ValueRO.width;
                gridCellSize = config.ValueRO.gridCellSize;
                gridSystemOrigin = config.ValueRO.originPosition;
                */


                // init grid mesh text
                //OnGridsInit?.Invoke(config.ValueRO.width, config.ValueRO.height, config.ValueRO.gridCellSize, buffer);



                Debug.Log($"Resized grid to {newSize}x{newSize}");
            }
        }
    }

    private void InitializeMissingPlayerPrefs(ref SystemState state) // update empty player pref with default value 
    {
        // For InitAnimalConfig
        foreach (var config in SystemAPI.Query<RefRO<InitAnimalConfig>>())
        {
            PlayerPrefs_GetAndSet("InitAnimalNum", config.ValueRO.initAnimalNumber);
            PlayerPrefs_GetAndSet("HuntThreshold", config.ValueRO.huntThreshold);
            PlayerPrefs_GetAndSet("MaxInitSize", config.ValueRO.maxInitSize);
            PlayerPrefs_GetAndSet("CompoundHuntThresholdRate", config.ValueRO.compoundHuntThresholdRate);
            PlayerPrefs_GetAndSet("AdultAgeError", config.ValueRO.adultAgeError);
            PlayerPrefs_GetAndSet("AdultDuration", config.ValueRO.adultDuration);
            PlayerPrefs_GetAndSet("AgingError", config.ValueRO.agingError);
            PlayerPrefs_GetAndSet("ReproductionCost", config.ValueRO.reproductionCost);
        }

        // For InitLakeConfig
        foreach (var config in SystemAPI.Query<RefRO<InitLakeConfig>>())
        {
            PlayerPrefs_GetAndSet("InitLakeNum", config.ValueRO.initLakeNumber);
        }

        // For InitHabitatConfig
        foreach (var config in SystemAPI.Query<RefRO<InitHabitatConfig>>())
        {
            PlayerPrefs_GetAndSet("NumOfHab", config.ValueRO.numberOfHabitat);
            PlayerPrefs_GetAndSet("EffectOfHiding", config.ValueRO.effectOfHiding);
            PlayerPrefs_GetAndSet("Capacity", config.ValueRO.capacity);
        }

        // For InitGridSystemConfig
        foreach (var config in SystemAPI.Query<RefRO<InitGridSystemConfig>>())
        {
            PlayerPrefs_GetAndSet("InitFieldSize", config.ValueRO.width);
        }

        // For AnimalBatch
        foreach (var config in SystemAPI.Query<RefRW<AnimalBatch>>())
        {
            PlayerPrefs_GetAndSet("BatchSize", config.ValueRO.BatchSize);
        }
    }

    private void UpdateConfigComponents(ref SystemState state)
    {
        Debug.Log("Update Config Components");


        // Update InitAnimalConfig
        foreach (var config in SystemAPI.Query<RefRW<InitAnimalConfig>>())
        {
            config.ValueRW.initAnimalNumber = PlayerPrefs.GetInt("InitAnimalNum", config.ValueRO.initAnimalNumber);
            config.ValueRW.huntThreshold = PlayerPrefs.GetFloat("HuntThreshold", config.ValueRO.huntThreshold);
            config.ValueRW.maxInitSize = PlayerPrefs.GetFloat("MaxInitSize", config.ValueRO.maxInitSize);
            config.ValueRW.compoundHuntThresholdRate = PlayerPrefs.GetFloat("CompoundHuntThresholdRate", config.ValueRO.compoundHuntThresholdRate);
            config.ValueRW.adultAgeError = PlayerPrefs.GetInt("AdultAgeError", config.ValueRO.adultAgeError);
            config.ValueRW.adultDuration = PlayerPrefs.GetInt("AdultDuration", config.ValueRO.adultDuration);
            config.ValueRW.agingError = PlayerPrefs.GetFloat("AgingError", config.ValueRO.agingError);
            config.ValueRW.reproductionCost = PlayerPrefs.GetFloat("ReproductionCost", config.ValueRO.reproductionCost);
        }

        // Update InitLakeConfig
        foreach (var config in SystemAPI.Query<RefRW<InitLakeConfig>>())
        {
            config.ValueRW.initLakeNumber = PlayerPrefs.GetInt("InitLakeNum", config.ValueRO.initLakeNumber);
        }

        // Update InitHabitatConfig
        foreach (var config in SystemAPI.Query<RefRW<InitHabitatConfig>>())
        {
            config.ValueRW.numberOfHabitat = PlayerPrefs.GetInt("NumOfHab", config.ValueRO.numberOfHabitat);
            config.ValueRW.effectOfHiding = PlayerPrefs.GetFloat("EffectOfHiding", config.ValueRO.effectOfHiding);
            config.ValueRW.capacity = PlayerPrefs.GetInt("Capacity", config.ValueRO.capacity);
        }

        // Update InitGridSystemConfig
        foreach (var config in SystemAPI.Query<RefRW<InitGridSystemConfig>>())
        {
            int fieldSize = PlayerPrefs.GetInt("InitFieldSize", config.ValueRO.width);
            config.ValueRW.width = fieldSize;
            config.ValueRW.height = fieldSize;
            config.ValueRW.remainingGrids = fieldSize * fieldSize;
        }

        foreach(var config in SystemAPI.Query<RefRW<AnimalBatch>>())
        {
            config.ValueRW.BatchSize = PlayerPrefs.GetInt("BatchSize", config.ValueRO.BatchSize);
            config.ValueRW.CurrentBatchIndex = 0;
            config.ValueRW.CycleCount = 0;
        }
    }

    // Helper methods for cleaner code
    private void PlayerPrefs_GetAndSet(string key, int defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, defaultValue);
        }
        else
        {
            // Still call Get to ensure the key is marked as used
            PlayerPrefs.GetInt(key, defaultValue);
        }

        PlayerPrefs.Save(); // Critical for builds!
    }

    private void PlayerPrefs_GetAndSet(string key, float defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, defaultValue);
        }
        else
        {
            PlayerPrefs.GetFloat(key, defaultValue);
        }

        PlayerPrefs.Save(); // Critical for builds!
    }
}