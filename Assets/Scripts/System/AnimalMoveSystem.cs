using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(InitAnimalConfigSystem))]
public partial struct AnimalMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Movement>();
    }

    public void OnDestroy(ref SystemState state) 
    {
    }



    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        uint randSeed = (uint)Random.Range(1, 114514);
        Debug.Log(randSeed);
        new MoveAnimalJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            //random = new Unity.Mathematics.Random(randSeed)
            randomSeed = randSeed,
        }.ScheduleParallel();
    }
}


//[BurstCompile]
public partial struct MoveAnimalJob : IJobEntity
{
    public float deltaTime;
    //public Unity.Mathematics.Random random;
    public uint randomSeed;

    //[BurstCompile]
    public void Execute(AnimalAspect animal)
    {
        animal.Move(deltaTime, randomSeed);
    }
}
