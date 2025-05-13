using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct RotatingCubeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

        state.RequireForUpdate<RotateSpeed>();//update when at least one RotateSpeed component exists
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;//disable this system
        return;


        /*
        foreach((RefRW<LocalTransform> localTransform, RefRW<RotateSpeed> rotateSpeed) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRW<RotateSpeed>>().WithNone<Player>())//withAll<RotatingCub>()
        {
            localTransform.ValueRW = localTransform.ValueRO.RotateY(rotateSpeed.ValueRO.value * SystemAPI.Time.DeltaTime);
        }
        */


        RotatingCubeJob rotatingCubeJob = new RotatingCubeJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        rotatingCubeJob.Schedule();//short form only works in IJobEntity
        //same as
        //systemState.Dependency = rotatingCubeJob.Schedule(systemState.Dependency); //full form

        //rotatingCubeJob.ScheduleParallel(); //only works when there are many entities
    }

    [BurstCompile]
    [WithNone(typeof(Player))]//ignore entity with player tag component //[WithAll(typeof(RotatingCub))]
    public partial struct RotatingCubeJob : IJobEntity
    {
        public float deltaTime; 


                               //ref: read write      in: read only
        public void Execute(ref LocalTransform localTransform, in RotateSpeed rotateSpeed)
        {
            localTransform = localTransform.RotateY(rotateSpeed.value * deltaTime);
        }
    }
   
}
