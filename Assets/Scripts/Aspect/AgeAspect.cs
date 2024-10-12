using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public readonly partial struct AgeAspect : IAspect
{
    public readonly Entity entity;

    public readonly RefRW<Age> _age;
    public readonly RefRW<AgeStage> _ageStage;



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
}
