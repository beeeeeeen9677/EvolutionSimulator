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
        bool sprinting = false;

        if (animal.IsThreatExist())
        {

            sprinting = animal.IsAbleToSprint(deltaTime); // check if sprint time remains


            float heading = MathHelpers.GetHeading(animal.threatPosition, animal.position);
            animal.FaceTarget(heading);
        }
        else if (animal.IsTargetExist())
        {

            sprinting = animal.IsAbleToSprint(deltaTime); // check if sprint time remains


            float heading = MathHelpers.GetHeading(animal.position, animal.targetPosition);
            //Debug.Log(animal.targetPosition);
            animal.FaceTarget(heading);
        }
        else
        {
            animal.RestoreSprintTime(deltaTime); // restore sprint time when no target & threat



            //animal.TurnRandomly(randomSeed);

            // go to habitat or turn randomly if do not have habitat
            bool isTakingRest = TakeRest(animal); // true: inside habitat

            if(isTakingRest)
            {
                return; // This acts like 'continue' in Execute()
            }


        }


        float movespeed = deltaTime * (sprinting ? animal.sprintSpeed : 1); // if sprinting, speed become higher


        animal.MoveForward(movespeed);
    }



    private bool TakeRest(AnimalAspect animal)
    {
        if(animal.IsHabitatExist())
        {
            float heading = MathHelpers.GetHeading(animal.position, animal.habitatPosition);
            animal.FaceTarget(heading);

            // whether inside/arrived habitat
            if(animal.IsInsideHabitat())
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            // Do not have habitat
            // walk randomly
            animal.TurnRandomly(randomSeed);

            return false;
        }
    }
}
