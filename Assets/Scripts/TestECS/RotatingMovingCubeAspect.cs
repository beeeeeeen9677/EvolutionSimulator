using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//Aspect: group different components into one Aspect
public readonly partial struct RotatingMovingCubeAspect : IAspect
{
    //can remove this TAG and add WithAll() in the loop
    public readonly RefRO<RotatingCube> rotatingCube;//TAG
    public readonly RefRW<LocalTransform> localTransform;
    public readonly RefRO<RotateSpeed> rotateSpeed;
    public readonly RefRO<TestMovement> movement;


    public void MoveAndRotate(float deltaTime)
    {
        localTransform.ValueRW = localTransform.ValueRO.RotateY(rotateSpeed.ValueRO.value * deltaTime);
        localTransform.ValueRW = localTransform.ValueRO.Translate(movement.ValueRO.movementVector * deltaTime);
    }
}
