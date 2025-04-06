using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public readonly RefRW<Cell> _cell;


    public readonly RefRW<Energy> _energy;
    public readonly RefRW<AnimalSensor> _animalSensor;
    public readonly RefRW<Target> _target;
    public readonly RefRW<Threat> _threat;

    public readonly RefRW<Sprint> _sprint;

    public readonly RefRO<AnimalHabitatInfo> _habitatInfo;

    public readonly RefRO<Family> _family;

    public readonly RefRW<Curiosity> _curiousity;


    //public readonly RefRO<SizeProperty> _sizeProperty;


    public float3 position
    {
        get => _localTransform.ValueRO.Position;
        private set => _localTransform.ValueRW.Position = value;
    }

  

    private float moveSpeed => _movement.ValueRO.speed;

    #region Cell
    // normal cell
    private int cell => _cell.ValueRO.numberOfNormalCell;
    // eat animal
    private int meatCell
    {
        get => _cell.ValueRO.numberOfMeatCell;
        set => _cell.ValueRW.numberOfMeatCell = value;  
    }
    // eat grass
    private int greenCell
    {
        get => _cell.ValueRO.numberOfGreenCell;
        set => _cell.ValueRW.numberOfGreenCell = value;
    }
    private int orangeCell
    {
        get => _cell.ValueRO.numberOfOrangeCell;
        set => _cell.ValueRW.numberOfOrangeCell = value;
    }
    private int purpleCell
    {
        get => _cell.ValueRO.numberOfPurpleCell;
        set => _cell.ValueRW.numberOfPurpleCell = value;
    }
    private int pinkCell
    {
        get => _cell.ValueRO.numberOfPinkCell;
        set => _cell.ValueRW.numberOfPinkCell = value;
    }
    #endregion



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


    


    private float originalMaxCooldown // trigger sensor
    {
        get => _animalSensor.ValueRO.maxCooldown;
        set => _animalSensor.ValueRW.maxCooldown = value;
    }




    private float currentCooldown
    {
        get => _animalSensor.ValueRO.currentCooldown;
        set => _animalSensor.ValueRW.currentCooldown = value;
    }


    public float grassSensorProbability
    {
        get => _animalSensor.ValueRO.grassSensorProbability;
        private set => _animalSensor.ValueRW.grassSensorProbability = value;
    }

    public float animalSensorProbability
    {
        get => _animalSensor.ValueRO.animalSensorProbability;
        private set => _animalSensor.ValueRW.animalSensorProbability = value;
    }


    private CollisionLayer? currentSensor
    {
        get => _animalSensor.ValueRO.currentSensor;
        set => _animalSensor.ValueRW.currentSensor = value;
    }

    private int greenWeight
    {
        get => _animalSensor.ValueRO.greenWeight;
        set => _animalSensor.ValueRW.greenWeight = value;
    }


    private int orangeWeight
    {
        get => _animalSensor.ValueRO.orangeWeight;
        set => _animalSensor.ValueRW.orangeWeight = value;
    }


    private int purpleWeight
    {
        get => _animalSensor.ValueRO.purpleWeight;
        set => _animalSensor.ValueRW.purpleWeight = value;
    }


    private int pinkWeight
    {
        get => _animalSensor.ValueRO.pinkWeight;
        set => _animalSensor.ValueRW.pinkWeight = value;
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

    #region habitat
    public HabitatProperty? habitatProperty
    {
        get => _habitatInfo.ValueRO.habitatProperty;
    }

    public Vector3 habitatPosition
    {
        get => _habitatInfo.ValueRO.habitatPosition;
    }
    #endregion



    /*
    public float initSize => _sizeProperty.ValueRO.initSize;
    public float maxSize => _sizeProperty.ValueRO.maxSize;
    */
    public float currentSize => _localTransform.ValueRO.Scale;



    // sprint effect time counter
    public float effectTimeCounter
    {
        get => _sprint.ValueRO.effectCounter;
        private set => _sprint.ValueRW.effectCounter = value;
    }
    // sprint effect total time 
    public float effectITotalTime => _sprint.ValueRW.effectTotalTime;



    public float curiousityProbability => _curiousity.ValueRO.probability;

    public float curiousityRemainTime
    {
        get => _curiousity.ValueRO.remainTime;
        private set => _curiousity.ValueRW.remainTime = value;
    }

    public float curiousityMaxTime
    {
        get => _curiousity.ValueRO.maxTime;
        private set => _curiousity.ValueRW.maxTime = value;
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

        if (IsInsideHabitat()) // if is inside habitat, consume less energy
        {
            deltaTime *= 0.1f;
        }

        // consume energy
        ModifyEnergy(-deltaTime);
        /*
        currentEnergy -= deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        */
        //Debug.Log("AnimalEnergy: " + currentEnergy);
    }


    // value will NOT affected by habitat 
    public void ConsumeFixedEnergy(float deltaTime)
    {
        ModifyEnergy(-deltaTime);
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




    public float GetSensorSize() // affercted by orange and pink cells
    {
        return sensorSize * cell * 0.0001f * (1 + 0.1f * Mathf.Log(1 + orangeCell) + 0.2f * Mathf.Log(1 + pinkCell)); 
    }

    public float GetOriginalSensorSize()
    {
        return sensorSize * cell * 0.0001f;
    }

    public float GetWarningRange() // affected by green & purple cells
    {
        return warningRange * cell * 0.0001f * (1 + 0.01f * Mathf.Log(1 + greenCell) + 0.1f * Mathf.Log(1 + purpleCell));
    }

    public float GetOriginalWarningRange()
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



    private float maxCooldown
    {
        get => _animalSensor.ValueRO.maxCooldown * (1 - 0.1f * Mathf.Log(1 + greenCell * 0.1f));
    }



    public bool SensorIsReady(float deltaTime) // cooldown, return whether CD is 0
    {
        // decrease Cooldown
        currentCooldown = Mathf.Clamp(currentCooldown - deltaTime, 0, maxCooldown);


        //Debug.Log("maxCooldown: " + maxCooldown);

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

            if(randResult < cumulativeProbability)
            {
                return i;
            }
        }

        Debug.LogError("Error: GetRandomSensorNumber() did not pick any of Sensor in for loop");
        return 0;
    }


    // get the sequence of different color of grasses
    public List<ColorCell> GetGrassColorSequence()
    {
        List<ColorCell> resultSequence = new List<ColorCell>();

        //initialize the List of weight for different colors
        List<ColorCell> remainingColor = new List<ColorCell> { ColorCell.Green, ColorCell.Orange, ColorCell.Purple, ColorCell.Pink};
        List<int> remainWeight = new List<int> { greenWeight, orangeWeight, purpleWeight, pinkWeight};

        // pick one color randomly, until the list become empty to form a sequence
        while (remainingColor.Count > 0)
        {
            // total weight of remaining weight
            int totalWeight = remainWeight.Sum();

            
            // random number for picking a color
            int randNum = UnityEngine.Random.Range(0, totalWeight);
            // cumulative weight, add the color's weight in this loop
            int cumulativeWeight = 0;

            for (int i = 0;i < remainWeight.Count; i++)
            {
                cumulativeWeight += remainWeight[i];

                if (randNum < cumulativeWeight)
                {
                    // if true, pick current color 
                    resultSequence.Add(remainingColor[i]); // Add current color to sequence list
                    // Remove from inital lists
                    remainingColor.RemoveAt(i);
                    remainWeight.RemoveAt(i);

                    break;
                }
                    // else check next color
                
            }
        }


        // print output
        
        string printColor = "";
        foreach(ColorCell color in resultSequence)
        {
            printColor += color.ToString() + "  "; 
        }

        Debug.Log(printColor);
        


        return resultSequence;
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

    // increase number of meat cell when eat meat by target size
    public void IncreaseMeatCell(int number)
    {
        meatCell += number;
    }

    public void IncreaseColorCell(int number, string grassColor)
    {
        switch (grassColor)
        {
            case "green":
                greenCell += number;
                break;
            case "orange":
                orangeCell += number;
                break;
            case "purple":
                purpleCell += number;
                break;
            case "pink":
                pinkCell += number;
                break;
            default:
                Debug.Log("IncreaseColorCell: no such color of grasses");
                break;
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
        //Debug.Log("ClearTarget: "+isSuccess);
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


    public void RestoreSprintTime(float deltaTime)
    {
        effectTimeCounter = Mathf.Clamp(effectTimeCounter + deltaTime * 0.25f, 0, effectITotalTime);
    }

    // use in move system, decide whether can sprint
    public bool IsAbleToSprint(float deltaTime)
    {
        if(effectTimeCounter > 0)
        {
            // able to sprint
            effectTimeCounter -= deltaTime;

            if (effectTimeCounter < 0)
                effectTimeCounter = 0;            

            return true;
        }
        else
        {
            return false;
        }
    }

    public float sprintSpeed => 1.5f * (1 + 0.1f * Mathf.Log(1 + meatCell));




    public bool IsHabitatExist()
    {
        return habitatProperty != null;
    }

    private float GetHabitatAreaRadius()
    {
        if (!IsHabitatExist())
        {// if habitat not exists
            return 0;
        }
        else
        {
            return habitatProperty.Value.radius;
        }
    }

    public bool IsInsideHabitat()
    {
        if (!IsHabitatExist()) // if do not have habitat
            return false;



        float distanceToHabitat = MathHelpers.GetDistance(position, habitatPosition);

        if(distanceToHabitat <= GetHabitatAreaRadius()/2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public Entity parent => _family.ValueRO.parent; 

    public bool IsFamily(AnimalAspect target) // check if target is family
    {
        // case 1: if target is parent
        if(target.entity == parent)
            return true;

        // case 2: if target is child
        if (target.parent == entity) // (if you are target's parent)
            return true;

        // case 3: if target is sibling
        if (target.parent == parent) // (if you and target have same parent)
            return true;


        return false;
    }


    public void TakeCuriousityAction()
    {
        // take curiousity action if habitat exist only
        if (!IsHabitatExist())
            return;

        if (currentEnergy < 0.7f * maxEnergy)
            return;


        // pooling for probability of curiousity action
        float curiousityProbResult = UnityEngine.Random.Range(0f, 1f);

        if (curiousityProbResult < curiousityProbability)
        {
            // take curiousity action
            curiousityRemainTime = curiousityMaxTime;

            Debug.Log("Take curiousity action");
        }
    }


    public bool IsCurious()
    {
        if (curiousityRemainTime > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void ConsumeCuriousityTime(float deltaTime)
    {
        curiousityRemainTime = Mathf.Clamp(curiousityRemainTime - deltaTime, 0, curiousityMaxTime);
    }
}
