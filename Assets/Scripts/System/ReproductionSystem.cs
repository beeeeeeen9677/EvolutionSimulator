using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct ReproductionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Energy>();
    }


    public void OnUpdate(ref SystemState state)
    {
        foreach ((GrowUpAspect animal, RefRO<Energy> energy) in SystemAPI.Query<GrowUpAspect, RefRO<Energy>>())
        {
            // only generate offspring in mature stage
            if (animal.currentStage != AgeStageEnum.mature)
                return;

            // if current energy more than 60% of max energy, born offspring
            if(energy.ValueRO.currentEnergy >= 0.5f * energy.ValueRO.maxEnergy)
            {
                if (animal.IsReadyToGenerateOffspring(SystemAPI.Time.DeltaTime))
                {
                    GenerateOffspring(animal.entity);
                }
            }
        }
    }

    private void GenerateOffspring(Entity entity)
    {
        Debug.Log("Born offspring");
    }
}
