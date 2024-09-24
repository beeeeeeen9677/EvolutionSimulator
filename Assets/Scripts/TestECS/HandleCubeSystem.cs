using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;



[UpdateAfter(typeof(SpawnCubeSystem))]
public partial struct HandleCubeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //state.Enabled = false;
        state.RequireForUpdate<RotatingCube>();//only do update when RotatingCube exist
    }
    
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("ASEd");
        foreach(RotatingMovingCubeAspect rotatingMovingCubeAspect in
            SystemAPI.Query<RotatingMovingCubeAspect>())
        {
            rotatingMovingCubeAspect.MoveAndRotate(SystemAPI.Time.DeltaTime);
            /*
            localTransform.ValueRW = localTransform.ValueRO.RotateY(rotateSpeed.ValueRO.value * SystemAPI.Time.DeltaTime);
            localTransform.ValueRW = localTransform.ValueRO.Translate(movement.ValueRO.movementVector * SystemAPI.Time.DeltaTime);
            */
        }
    }
}
