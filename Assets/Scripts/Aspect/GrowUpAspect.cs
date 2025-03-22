using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public readonly partial struct GrowUpAspect : IAspect
{
    public readonly Entity entity;

    public readonly RefRW<LocalTransform> _localTransform;
    public readonly RefRW<Age> _age;
    public readonly RefRW<AgeStage> _ageStage;
    public readonly RefRW<Cell> _cell;
    public readonly RefRW<SizeProperty> _sizeProperty;
    public readonly RefRW<ReproductionCounter> _reproductionCounter;



    public float currentAge
    {
        get => _age.ValueRO.currentAge;
        private set => _age.ValueRW.currentAge = value;
    }

    public int matureThreshold
    {
        get => _ageStage.ValueRO.matureThreshold;
        private set => _ageStage.ValueRW.matureThreshold = value;
    }

    public int agingThreshold
    {
        get => _ageStage.ValueRO.agingThreshold;
        private set => _ageStage.ValueRW.agingThreshold = value;
    }

    public AgeStageEnum currentStage
    {
        get => _ageStage.ValueRO.currentStage;
        private set => _ageStage.ValueRW.currentStage = value;
    }

    public int cell
    {
        get => _cell.ValueRO.numberOfNormalCell;
        private set => _cell.ValueRW.numberOfNormalCell = value;
    }


    public float initSize => _sizeProperty.ValueRO.initSize;
    public float maxSize => _sizeProperty.ValueRO.maxSize;
    public float currentSize
    {
        get => _sizeProperty.ValueRO.currentSize;
        private set
        {
            _sizeProperty.ValueRW.currentSize = value;
            _localTransform.ValueRW.Scale = value;
        }
    } 


    public float reproductionInterval => _reproductionCounter.ValueRO.interval;
    public float reproductionCD
    {
        get => _reproductionCounter.ValueRO.currentCD;
        private set => _reproductionCounter.ValueRW.currentCD = value;
    }







    public void IncreaseAge(float deltaTime)
    {
        // increase age value
        currentAge += deltaTime;

        // update current age stage
        if (currentAge >= agingThreshold)
        {
            currentStage = AgeStageEnum.aging;
        }
        else if (currentAge >= matureThreshold)
        {
            currentStage = AgeStageEnum.mature;
        }
    }


    public void UpdateNumberOfCell(float deltaTime)
    {
        if(currentStage == AgeStageEnum.infant)
        {
            cell += (int)(deltaTime * 10000); // deltaTime = 0.004 in most of the time
        }

        if (currentStage != AgeStageEnum.aging) 
        {
            //if()
        }



        if (currentStage == AgeStageEnum.aging)
        {    
            cell -= (int)(deltaTime * 1000);

            // wont smaller than 100
            if(cell < 100)
                cell = 100;
        }
    }


    public void UpdateSize() // update size by referring to age
    {
        // only will scale up in infant stage
        if (currentStage == AgeStageEnum.infant)
        {
            currentSize = Mathf.Clamp(
                initSize + (currentAge / matureThreshold) * (maxSize - initSize),
                initSize, maxSize);
        }
        else if(currentStage == AgeStageEnum.mature)
        {
            // max size when mature
            currentSize = maxSize;
        }
    }


    public bool IsReadyToGenerateOffspring(float deltaTime) // Count down, return true if CD = 0
    {
        // decrease CD
        reproductionCD -= deltaTime;

        if(reproductionCD <= 0) // ready to born offspring
        {
            reproductionCD = reproductionInterval;
            return true;
        }
        else
        {
            return false;
        }
    }
}
