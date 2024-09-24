using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct AnimalAspect : IAspect
{
    public readonly Entity entity;

    public readonly RefRW<LocalTransform> _localTransform;
    public readonly RefRW<Movement> _movement;


    private float moveSpeed => _movement.ValueRO.speed;

    // Move Randomly
    public void Move(float deltaTime, uint randSeed)
    {
        float maxTurnAngle = 0.15f;

        // Create an instance of the random number generator
        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(moveSpeed + randSeed * entity.Index));
        float rotateResult = random.NextFloat(-maxTurnAngle, maxTurnAngle);
        //Debug.Log(rotateResult);
        _localTransform.ValueRW = _localTransform.ValueRO.RotateY(random.NextFloat(-maxTurnAngle, maxTurnAngle));
        //_localTransform.ValueRW.RotateY(random.NextFloat(-turnAngle, turnAngle));


        _localTransform.ValueRW.Position += _localTransform.ValueRO.Forward() * moveSpeed * deltaTime;
    }
}
