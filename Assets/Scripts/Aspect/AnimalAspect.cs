using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public readonly partial struct AnimalAspect : IAspect
{
    public readonly Entity entity;

    public readonly RefRW<LocalTransform> _localTransform;
    public readonly RefRW<Movement> _movement;
    public readonly RefRW<PhysicsVelocity> _physicsVelocity;
    public readonly RefRO<Cell> _cell;


    public readonly RefRW<Energy> _energy;
    public readonly RefRW<AnimalSensor> _animalSensor;
    public readonly RefRW<Target> _target;




    private float moveSpeed => _movement.ValueRO.speed;
    private int cell => _cell.ValueRO.numberOfCell;
    private float maxEnergy => _energy.ValueRO.maxEnergy;
    private float currentEnergy
    {
        get => _energy.ValueRO.currentEnergy;
        set => _energy.ValueRW.currentEnergy = value;
    }


    private float sensorSize
    {
        get => _animalSensor.ValueRO.size;
        set => _animalSensor.ValueRW.size = value;
    }




    private Entity targetEntity
    {
        get => _target.ValueRO.targetEntity;
        set => _target.ValueRW.targetEntity = value;    
    }
    public float3 targetPosition
    {
        get => _target.ValueRO.targetPosition;
        set => _target.ValueRW.targetPosition = value;
    }



    public void FaceTarget(float heading)
    {
        // set the x y z axis of target object's rotation
        _localTransform.ValueRW.Rotation = quaternion.Euler(0, heading, 0); 
    }



    // Trun Randomly
    public void TurnRandomly(uint randSeed)
    {
        float maxTurnAngle = 0.15f;


        
        // Create an instance of the random number generator
        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(moveSpeed + randSeed * entity.Index));
        float rotateResult = random.NextFloat(-maxTurnAngle, maxTurnAngle);
        //Debug.Log("Origin: "+rotateResult+" "+ _localTransform.ValueRO.Rotation.value.y*360);
        // random rotation / turn
        _localTransform.ValueRW = _localTransform.ValueRO.RotateY(rotateResult);

  



        // freeze x/z rotation
        quaternion currentRotation = _localTransform.ValueRO.Rotation;
        float3 tempEuler = math.degrees(math.Euler(currentRotation));
        tempEuler.x = 0;  // Clamp X
        tempEuler.z = 0;  // Clamp Z
        _localTransform.ValueRW.Rotation = quaternion.Euler(math.radians(tempEuler));  // Set rotation back
    
    }


    public void MoveForward(float deltaTime)
    {
        // move
        _localTransform.ValueRW.Position += _localTransform.ValueRO.Forward() * moveSpeed * deltaTime * cell * 0.00001f;

        Debug.Log(moveSpeed * deltaTime * cell * 0.00001f);


        //rotateResult = random.NextFloat(-maxTurnAngle, maxTurnAngle) + _localTransform.ValueRO.Rotation.value.y;
        //Debug.Log("New: "+rotateResult);
        //_localTransform.ValueRW.Rotation = quaternion.Euler(0, rotateResult, 0);
        //_physicsVelocity.ValueRW.Linear = _localTransform.ValueRO.Forward() * moveSpeed * deltaTime * 500;

    }


    // Consume Energy
    public void ConsumeEnergy(float deltaTime)
    {
        // consume energy
        ModifyEnergy(-deltaTime);
        /*
        currentEnergy -= deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        */
        //Debug.Log("AnimalEnergy: " + currentEnergy);
    }


    // for increase / decrease energy with clamp checking
    private void ModifyEnergy(float value)
    {
        currentEnergy += value;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }



    public float GetEnergy() { return currentEnergy; }  

    // check if any target is being locked on
    public bool IsTargetExist()
    {
        return targetEntity != Entity.Null;
    }

    public Entity GetTargetEntity() { return targetEntity; }
    public void SetTargetEntity(Entity targetEntity)
    {
        this.targetEntity = targetEntity;
    }
    public float GetSensorSize()
    {
        return sensorSize; 
    }

    public void EatTarget(float obtainedEnergy)
    {
        //Debug.Log("Eat");
        ModifyEnergy(obtainedEnergy);
        /*
        currentEnergy += obtainedEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        */
    }

    public void ClearTarget()
    {
        targetEntity = Entity.Null;
    }
}
