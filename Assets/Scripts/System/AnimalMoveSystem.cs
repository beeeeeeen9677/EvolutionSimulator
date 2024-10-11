using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
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
        //var localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>(true);
        uint randSeed = (uint)Random.Range(1, 114514);
        //Debug.Log(randSeed);
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
        if (animal.IsTargetExist())
        {

            float heading = MathHelpers.GetHeading(animal._localTransform.ValueRO.Position, animal.targetPosition);
            //Debug.Log(animal.targetPosition);
            animal.FaceTarget(heading);
        }
        else
        {
            animal.TurnRandomly(randomSeed);
        }

        animal.MoveForward(deltaTime);
    }
}
