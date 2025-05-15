using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



[UpdateAfter(typeof(ConfigUpdateSystem))]
[UpdateAfter(typeof(InitLakeSystem))]
public partial class GridUpdateSystem : SystemBase
{
    private bool initialized;


    public Action<int, int, int, DynamicBuffer<GridCell>> OnGridsInit;
    public Action<int, int, string> OnOneGridCellValueChanged;
    public Action<DynamicBuffer<GridCell>> OnAllGridCellValueChanged;
    public Action<ExportData> OnAvgDataExport;
    public Action<ExportData> OnSeperrateDataExport;




    Vector3 gridSystemOrigin;
    int gridBufferWidth;
    int gridCellSize;



    protected override void OnCreate()
    {
        RequireForUpdate<InitGridSystemConfig>();
        initialized = false;
    }


    protected override void OnUpdate()
    {
        Enabled = false;
        // Initiation, execute only once
        if (!initialized)
        {




            // grid system config var
            var initGridSystemConfig = SystemAPI.GetSingleton<InitGridSystemConfig>();
            Entity initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
            DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
            gridBufferWidth = initGridSystemConfig.width;
            gridCellSize = initGridSystemConfig.gridCellSize;
            gridSystemOrigin = initGridSystemConfig.originPosition;



            // init grid mesh text
            OnGridsInit?.Invoke(gridBufferWidth, initGridSystemConfig.height, gridCellSize, gridCellBuffer);



            Debug.Log("Init Diffusion");

            // Init water diffusion
            int initDiffusionTime = 0; // Do 3 times at first
            for (int i = 0; i < initDiffusionTime; i++)
            {
                UpdateGridCellsInformation();
            }


            OnAllGridCellValueChanged?.Invoke(gridCellBuffer);


            initialized = true;
        }


        /*
        // Usual update by pressing Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);

            DiffuseSoilWater();
        }
        */

    }

    public void UpdateGridCellsInformation(int day = 0)
    {

        /*
        RefRW<InitGridSystemConfig> initGridSystemConfig;


        //Debug.Log("Diffuse Soil Water");

        DynamicBuffer<GridCell> gridCellBuffer;

        try  // Always crash here
        {
            if (initGridSystemConfigEntity == Entity.Null)
            {
                initGridSystemConfigEntity = SystemAPI.GetSingletonEntity<InitGridSystemConfig>();
            }
            initGridSystemConfig = SystemAPI.GetSingletonRW<InitGridSystemConfig>();
            gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);
        }
        catch (ArgumentException)
        {
            Debug.LogError("Grid Update System - UpdateGridCellsInformation: Wait for next update due to ArgumentException. ");
            return;
        }
        catch (Exception)
        {
            Debug.LogError("Grid Update System - Unknown Exception. ");
            return;
        }
        */

        foreach (var (initGridSystemConfig, initGridSystemConfigEntity) in
            SystemAPI.Query<RefRW<InitGridSystemConfig>>().WithEntityAccess())
        {
            // Debug.Log("Diffuse Soil Water, day: "+day);



            DynamicBuffer<GridCell> gridCellBuffer = EntityManager.GetBuffer<GridCell>(initGridSystemConfigEntity);



            // Debug.Log("Grid Cell Buffer Length: " );
            int diffusionMode = 2;



            // Stack for storing all modification records, excute them outside the loop
            List<ModifyGridMoistureRecord> modifyMoistureStack = new List<ModifyGridMoistureRecord>();


            // loop through all grid cells
            foreach (GridCell centerGridCell in gridCellBuffer)
            {


                // Spawn new object on empty grid randomly
                if (centerGridCell.storingObject == Entity.Null) // storing nothing
                {

                    int maxProbNum = 100000; // probability for spawning a new grass
                    if (UnityEngine.Random.Range(0, maxProbNum) < 1 && initGridSystemConfig.ValueRO.remainingGrids > 0) // spawn a new grass if fulfill probability and grid remains
                    {
                        initGridSystemConfig.ValueRW.remainingGrids--;
                        //Debug.Log("Placed new object (Grass)");


                        InitGrassConfig initGrassConfig = SystemAPI.GetSingleton<InitGrassConfig>();
                        Entity initGrassConfigEntity = SystemAPI.GetSingletonEntity<InitGrassConfig>();
                        var grassTypeBuffer = EntityManager.GetBuffer<GrassPrefabElement>(initGrassConfigEntity);

                        // get random type of grasses
                        int grassIndex = UnityEngine.Random.Range(0, grassTypeBuffer.Length);
                        Entity newSpawnedGrass = EntityManager.Instantiate(grassTypeBuffer[grassIndex].grassPrefabs);

                        // Set the new grass into the grid storing object property
                        GridBufferUtils.SetGridCell(gridCellBuffer, gridBufferWidth, centerGridCell.X, centerGridCell.Y, newSpawnedGrass);

                        Vector3 newGrassPosition = GridBufferUtils.GetWorldPosition(centerGridCell.X, centerGridCell.Y, gridCellSize, gridSystemOrigin);

                        SystemAPI.SetComponent(newSpawnedGrass, new LocalTransform
                        {
                            Position = newGrassPosition,
                            Rotation = quaternion.identity,
                            Scale = 1
                        });

                        // set grass property
                        GrassProperties grassPropertyData = SystemAPI.GetComponent<GrassProperties>(newSpawnedGrass);
                        float randomMaxSize = UnityEngine.Random.Range(0.7f, 1f);

                        RefRW<GrassProperties> grassProperties = SystemAPI.GetComponentRW<GrassProperties>(newSpawnedGrass);
                        grassProperties.ValueRW.currentSize = 0;
                        grassProperties.ValueRW.maxSize = randomMaxSize;
                        grassProperties.ValueRW.provideEnergy = grassPropertyData.provideEnergy;
                        grassProperties.ValueRW.activated = grassPropertyData.activated;

                    }
                }











                // Update grass variables     AND     Consume Moisture and Nutrient
                if (SystemAPI.HasComponent<GrassProperties>(centerGridCell.storingObject))  // check if there is grass on the grid cell
                {
                    // if yes, update the moisture and nutrient of the grass

                    GrassAspect grassAspect = SystemAPI.GetAspect<GrassAspect>(centerGridCell.storingObject);

                    grassAspect.grid_Moisture = centerGridCell.soilMoisture_value;
                    grassAspect.grid_Nutrient = centerGridCell.soilNutrient;


                    // Consume Moisture and Nutrient
                    float cr_Moi = PlayerPrefs.GetFloat("ConsumeRate_Moi", 0.001f);
                    PlayerPrefs.SetFloat("ConsumeRate_Moi", cr_Moi);

                    float consumeRate_Moi = cr_Moi * grassAspect.growthRate_Moisture;

                    GridBufferUtils.SetGridCell(gridCellBuffer, gridBufferWidth, centerGridCell.X, centerGridCell.Y, centerGridCell.storingObject, centerGridCell.soilMoisture_value - consumeRate_Moi);




                    float cr_Ntr = PlayerPrefs.GetFloat("ConsumeRate_Ntr", 0.001f);
                    PlayerPrefs.SetFloat("ConsumeRate_Ntr", cr_Ntr);

                    float consumeRate_Ntr = cr_Ntr * grassAspect.growthRate_Nutrient;

                    GridBufferUtils.AddGridNutrient(gridCellBuffer, gridBufferWidth, centerGridCell.X, centerGridCell.Y, -1 * consumeRate_Ntr);

                }



                // Assign nutrient to grids from Tree (shelter)
                if (SystemAPI.HasComponent<HabitatProperty>(centerGridCell.storingObject))
                {
                    // if yes, update the nutrient of surrounding grids
                    int range = 3;
                    List<GridCell> surroundingGridList = GridBufferUtils.GetSurroundingGridCellsInSquare(gridCellBuffer, gridBufferWidth, range, centerGridCell.X, centerGridCell.Y);
                    foreach (GridCell cell in surroundingGridList)
                    {
                        // add nutrient for grids having less than 1 of nutrient only
                        if (cell.soilNutrient >= 1)
                            continue;

                        GridBufferUtils.AddGridNutrient(gridCellBuffer, gridBufferWidth, cell.X, cell.Y, 0.001f);
                    }

                    // Debug.Log("Tree: Assign Nutrient");
                }




                // Update moisture when higher/lower than 0.6 density
                float modifyRate = 0.0001f;
                if (centerGridCell.soilMoisture_value <= centerGridCell.soilDensity * 0.6f)
                {
                    // decrease density if lower than
                    modifyRate *= -10;
                }
                // else increase
                GridBufferUtils.ModifyGridDensity(gridCellBuffer, gridBufferWidth, centerGridCell.X, centerGridCell.Y, modifyRate + centerGridCell.soilDensity);
                // Debug.Log("Update Density: " + modifyRate);





                float diffusionThreshold = centerGridCell.soilDensity + centerGridCell.soilNutrient * 0.1f;

                // Diffusion

                if (IsValidToDiffuse(diffusionMode, centerGridCell.soilMoisture_value, diffusionThreshold)) // at least larger than threshold to diffuse
                {
                    // get surrounding grids of the lake (center grid) by lake range
                    List<GridCell> surroundingGridList = GridBufferUtils.GetSurroundingGridCells(gridCellBuffer, gridBufferWidth, 1, centerGridCell.X, centerGridCell.Y);

                    //Debug.Log("Moisture: " + centerGridCell.soilMoisture_value + "   " + surroundingGridList.Count);






                    // Set the soil moisture of surrounding grids to half of center grid
                    foreach (GridCell surroundingGridCell in surroundingGridList)
                    {


                        float moistureToBeAdded = 0;


                        #region Diffusion Mode 1 
                        if (diffusionMode == 1)
                        {
                            // Minecraft water diffusion logic, water of source grid will NOT be decreased

                            // the moisture of grid to be diffused should not be higher than half of moisture of center grid
                            if (surroundingGridCell.soilMoisture_value > centerGridCell.soilMoisture_flooredValue / 2)
                                continue;
                            // if moisture of surrounding ?? half of center
                            //  = :  add 1/4 center moisture
                            //  < :  add 1/2 center moisture
                            moistureToBeAdded = centerGridCell.soilMoisture_flooredValue /
                                ((surroundingGridCell.soilMoisture_value == centerGridCell.soilMoisture_flooredValue / 2) ? 4 : 2);

                        }
                        #endregion



                        #region Diffusion Mode 2
                        if (diffusionMode == 2)
                        {
                            // water of source grid will be decreased



                            // the moisture of grid to be diffused should not be higher than moisture of center grid
                            if (surroundingGridCell.soilMoisture_value > centerGridCell.soilMoisture_value)
                                continue;

                            float diffusionScaler = PlayerPrefs.GetFloat("DiffusionScaler", 0.05f);
                            PlayerPrefs.SetFloat("DiffusionScaler", diffusionScaler);


                            moistureToBeAdded = centerGridCell.soilMoisture_flooredValue * diffusionScaler;
                            // decrease water from the source
                            modifyMoistureStack.Add(new ModifyGridMoistureRecord(centerGridCell.X, centerGridCell.Y, -1 * moistureToBeAdded));
                        }
                        #endregion


                        if (moistureToBeAdded == 0)
                        {
                            Debug.Log("moisture To Be Added = 0");
                        }


                        //GridBufferUtils.SetGridCell(gridCellBuffer, gridBufferWidth, surroundingGridCell.X, surroundingGridCell.Y, surroundingGridCell.storingObject, centerGridCell.soilMoisture_flooredValue / 2);

                        // add water to the surrounding grid
                        modifyMoistureStack.Add(new ModifyGridMoistureRecord(surroundingGridCell.X, surroundingGridCell.Y, moistureToBeAdded));

                        //OnOneGridCellValueChanged?.Invoke(surroundingGridCell.X, surroundingGridCell.Y, (centerGridCell.soilMoisture_flooredValue / 2).ToString());
                    }

                }
            }


            // execute all stacked modification records
            GridBufferUtils.ExecuteStackedRecord(gridCellBuffer, gridBufferWidth, modifyMoistureStack);

            OnAllGridCellValueChanged?.Invoke(gridCellBuffer);



            if (day % 100 == 0) // export for every 100 days
            {
                ExportData exportData = RecordSeparateAnimalData(day);
                // Debug.Log($"Day: {exportData.day}   Speed: {exportData.moveSpeed}");
                OnAvgDataExport?.Invoke(exportData);
            }
        }
    }


    private bool IsValidToDiffuse(int diffusionMode, float moistureValue, float diffusionThreshold)
    {
        if (diffusionThreshold < 1)
            diffusionThreshold = 1;



        switch (diffusionMode)
        {
            // Mode 1: > 2          Mode 2: >= 2
            // at least larger than 2 unit of moisture to diffuse
            case 1:
                if (moistureValue > diffusionThreshold)
                {
                    //validToDiffuse = true;
                    return true;
                }
                break;

            case 2:
                if (moistureValue >= diffusionThreshold)
                {
                    //validToDiffuse = true;
                    return true;
                }
                break;

            default:
                Debug.Log("Invalid Diffusion Mode: " + diffusionMode);
                break;
        }

        return false;
    }



    public ExportData CalculateAverageAnimalData(int simulationDay)
    {
        var query = EntityManager.CreateEntityQuery(typeof(AnimalTag)); // Adjust query as needed
        var entityCount = query.CalculateEntityCount();

        if (entityCount == 0)
            return new ExportData();

        var averages = new NativeArray<float>(4, Allocator.TempJob); // [moveSpeed, maxEnergy, size, sensorProb]

        new CalculateAveragesJob
        {
            Averages = averages
        }
        .Schedule(new JobHandle())
        .Complete();




        var result = new ExportData
        {
            day = simulationDay,
            moveSpeed = averages[0] / entityCount,
            maxEnergy = averages[1] / entityCount,
            size = averages[2] / entityCount,
            animalSensorProb = averages[3] / entityCount
        };

        averages.Dispose();
        return result;
    }

    public ExportData RecordSeparateAnimalData(int simulationDay)
    {
        var query = SystemAPI.QueryBuilder().WithAll<AnimalTag>().Build();
        var entityCount = query.CalculateEntityCount();

        if (entityCount == 0)
            return new ExportData();

        // Use NativeList to collect all animal data
        var animalData = new NativeList<FixedString128Bytes>(entityCount, Allocator.TempJob);

        // Schedule job to collect individual animal data
        new RecordAnimalSeparateDataJob
        {
            Day = simulationDay,
            AnimalData = animalData
        }
        .Schedule(new JobHandle())
        .Complete();



        // Combine all records into one string with newlines
        var result = new ExportData
        {
            day = simulationDay,
            separateData = string.Join("\n", animalData.AsArray().ToArray())
        };

        animalData.Dispose();
        return result;
    }
}






// for storing records of soil moisture modification
public class ModifyGridMoistureRecord
{
    //public GridCell gridCell { get; private set; } // cannot use struct since it is value type
    public int X { get; private set; } // XY coordinate of grid cell to be modified
    public int Y { get; private set; }
    public float moistureToBeAdded { get; private set; } // value of moisture to be added after modified


    public ModifyGridMoistureRecord(int X, int Y, float moistureToBeAdded)
    {
        this.X = X;
        this.Y = Y;
        this.moistureToBeAdded = moistureToBeAdded;
    }
}

// not using
public partial struct CalculateAveragesJob : IJobEntity
{
    public NativeArray<float> Averages;

    public void Execute(AnimalAspect animal)
    {
        Averages[0] += animal.baseMoveSpeed;
        Averages[1] += animal.maxEnergy;
        Averages[2] += animal.maxSize;
        Averages[3] += animal.animalSensorProbability;
    }
}

partial struct RecordAnimalSeparateDataJob : IJobEntity
{
    public int Day;
    public NativeList<FixedString128Bytes> AnimalData;


    void Execute(AnimalAspect animal)
    {
        // Format: "day,moveSpeed,maxEnergy,size,sensorProb" per animal
        AnimalData.Add(new FixedString128Bytes(
            $"{Day},{animal.baseMoveSpeed},{animal.maxEnergy},{animal.maxSize},{animal.animalSensorProbability},{animal.sensorSize},{animal.warningRange},{animal.entity.Index}"
        ));
    }
}
