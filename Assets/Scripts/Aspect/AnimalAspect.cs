using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public readonly partial struct AnimalAspect : IAspect
{
    public readonly Entity entity;

    public readonly RefRW<LocalTransform> _localTransform;
    public readonly RefRW<Movement> _movement;
    //public readonly RefRW<PhysicsVelocity> _physicsVelocity;
    public readonly RefRO<Cell> _cell;


    public readonly RefRW<Energy> _energy;
    public readonly RefRW<AnimalSensor> _animalSensor;
    public readonly RefRW<Target> _target;
    public readonly RefRW<Threat> _threat;

    //public readonly RefRO<SizeProperty> _sizeProperty;


    public float3 position
    {
        get => _localTransform.ValueRO.Position;
        private set => _localTransform.ValueRW.Position = value;
    }


    private float moveSpeed => _movement.ValueRO.speed;
    private int cell => _cell.ValueRO.numberOfCell;
    public float maxEnergy => _energy.ValueRO.maxEnergy;
    private float currentEnergy
    {
        get => _energy.ValueRO.currentEnergy;
        set => _energy.ValueRW.currentEnergy = value;
    }



    #region Animal_Sensor
    private float sensorSize
    {
        get => _animalSensor.ValueRO.size;
        set => _animalSensor.ValueRW.size = value;
    }


    private float warningRange
    {
        get => _animalSensor.ValueRO.warningRange;
        set => _animalSensor.ValueRW.warningRange = value;
    }


    private float maxCooldown
    {
        get => _animalSensor.ValueRO.maxCooldown;
        set => _animalSensor.ValueRW.maxCooldown = value;
    }


    private float currentCooldown
    {
        get => _animalSensor.ValueRO.currentCooldown;
        set => _animalSensor.ValueRW.currentCooldown = value;
    }


    private float grassSensorProbability
    {
        get => _animalSensor.ValueRO.grassSensorProbability;
        set => _animalSensor.ValueRW.grassSensorProbability = value;
    }

    private float animalSensorProbability
    {
        get => _animalSensor.ValueRO.animalSensorProbability;
        set => _animalSensor.ValueRW.animalSensorProbability = value;
    }


    private CollisionLayer? currentSensor
    {
        get => _animalSensor.ValueRO.currentSensor;
        set => _animalSensor.ValueRW.currentSensor = value;
    }
    #endregion



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

    private float maxChaseTime
    {
        get => _target.ValueRO.maxChaseTime;
        set => _target.ValueRW.maxChaseTime = value;
    }


    public float remainChaseTime
    {
        get => _target.ValueRO.remainChaseTime;
        private set => _target.ValueRW.remainChaseTime = value;
    }


    public Entity threatEntity
    {
        get => _threat.ValueRO.threatEntity;
        private set => _threat.ValueRW.threatEntity = value;
    }



    public float3 threatPosition
    {
        get => _threat.ValueRO.threatPosition;
        set => _threat.ValueRW.threatPosition = value;
    }





    /*
    public float initSize => _sizeProperty.ValueRO.initSize;
    public float maxSize => _sizeProperty.ValueRO.maxSize;
    */
    public float currentSize => _localTransform.ValueRO.Scale;



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

    public float GetMoveSpeed()
    {
        return moveSpeed * cell * 0.00001f;
    }


    public void MoveForward(float deltaTime)
    {
        // move
        position += _localTransform.ValueRO.Forward() * deltaTime * GetMoveSpeed();

        //Debug.Log(moveSpeed * deltaTime * cell * 0.00001f);


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
        currentEnergy = Mathf.Clamp(currentEnergy + value, 0, maxEnergy);
    }



    public float GetEnergy() 
    { 
        return currentEnergy; 
    }  

    // check if any target is being locked on
    public bool IsTargetExist()
    {
        return targetEntity != Entity.Null;
    }

    public Entity GetTargetEntity() 
    { 
        return targetEntity; 
    }

    public void SetTargetEntity(Entity targetEntity)
    {
        this.targetEntity = targetEntity;
    }


    public void ResetChaseCountdown()
    {
        remainChaseTime = maxChaseTime;
    }


    public bool IsChaseTimeOver(float deltaTime) // true if counter less than 0
    {
        remainChaseTime = Mathf.Clamp(remainChaseTime - deltaTime, 0, maxChaseTime);

        return remainChaseTime <= 0;   
    }


    public void SetCurrentSensor(CollisionLayer collisionLayer)
    {
        currentSensor = collisionLayer;
    }

    public void ResetCurrentSensor()
    {
        currentSensor = null;
    }




    public float GetSensorSize()
    {
        return sensorSize * cell * 0.0001f; 
    }

    public float GetWarningRange()
    {
        return warningRange * cell * 0.0001f;
    }


  

    public void SetThreatEntity(Entity threat, float3 threatPos)
    {
        // set threat entity only if no exisiting threatEntity

        if (threatEntity != Entity.Null)
            return;

        threatEntity = threat;
        threatPosition = threatPos;
    }


    public bool IsThreatExist()
    {

        // if no threat exist
        if (threatEntity == Entity.Null)
        {
            //Debug.Log("No Threat");
            return false;
        }
        // if threat ourside warning range
        else if (MathHelpers.GetDistance(position, targetPosition) > GetWarningRange())
        {
            //Debug.Log($"Threat outside: {MathHelpers.GetDistance(position, targetPosition)} > {GetWarningRange()}");
            return false;
        }


        //Debug.Log("Threat Found");
        return true;
    }
    

    // call by threat entity
    public void ClearThreat(Entity threat)
    {
        // clear only if called by threat entity
        if(threatEntity == threat)
            threatEntity = Entity.Null;
    }




    public bool SensorIsReady(float deltaTime) // cooldown, return whether CD is 0
    {
        // decrease Cooldown
        currentCooldown = Mathf.Clamp(currentCooldown - deltaTime, 0, maxCooldown);

        // check if finished cooldown 
        if (currentCooldown == 0)
        {
            currentCooldown = maxCooldown; // reset CD
            return true;
        }

        // not ready to use
        return false;
    }


    public int GetRandomSensorNumber()
    {
        float randResult = UnityEngine.Random.Range(0f, 1f);

        float[] sensorProbs = { grassSensorProbability, animalSensorProbability};
        float cumulativeProbability = 0;
        for(int i = 0; i < sensorProbs.Length; i++)
        {
            cumulativeProbability += sensorProbs[i];

            if(cumulativeProbability > randResult)
            {
                return i;
            }
        }

        Debug.LogError("Error: GetRandomSensorNumber() did not pick any of Sensor in for loop");
        return 0;
    }


    public void AdjustSensorProbability(bool isSuccess) //Call after every round of scanning & hunting
    {
        float adjustRate = 0.1f;
        if(currentSensor == CollisionLayer.Grass)
        {
            if (isSuccess) // success to eat grass
            {
                grassSensorProbability = Mathf.Clamp(grassSensorProbability + adjustRate, 0, 1);
                animalSensorProbability = 1 - grassSensorProbability;
            }
            else // fail to eat grass
            {
                grassSensorProbability = Mathf.Clamp(grassSensorProbability - adjustRate, 0, 1);
                animalSensorProbability = 1 - grassSensorProbability;
            }
        }
        else if (currentSensor == CollisionLayer.Animal)
        {
            if (isSuccess) // success to eat animal
            {
                animalSensorProbability = Mathf.Clamp(animalSensorProbability + adjustRate, 0, 1);
                grassSensorProbability = 1 - animalSensorProbability;
            }
            else // fail to eat animal
            {
                animalSensorProbability = Mathf.Clamp(animalSensorProbability - adjustRate, 0, 1);
                grassSensorProbability = 1 - animalSensorProbability;
            }
        }
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

    public void ClearTarget(bool isSuccess) // isSuccess: this round success to eat target?
    {
        Debug.Log("ClearTarget: "+isSuccess);
        targetEntity = Entity.Null;
        //remainChaseTime = 0;


        AdjustSensorProbability(isSuccess);
    }


    public bool IsAbleToHuntTarget(AnimalAspect targetEntity) // return True if able to hunt
    {
        return 0.7f * currentSize >= targetEntity.currentSize; // compare size
    }


    // call when this animal was eaten by others
    public void WasEaten()
    {
        currentEnergy = 0;
    }
}
