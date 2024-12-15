using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct HungerStatusSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HungerStatus>();
        state.RequireForUpdate<Energy>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach(var(hungerStatus, energy) in SystemAPI.Query<RefRW<HungerStatus>, RefRO<Energy>>())
        {
            // if current energy larger than certain value -> full, else hungry
            if(energy.ValueRO.currentEnergy >= hungerStatus.ValueRO.hungryThreshold * energy.ValueRO.maxEnergy)
            {
                hungerStatus.ValueRW.currentState = HungerStatusEnum.full;
            }
            else
            {
                hungerStatus.ValueRW.currentState = HungerStatusEnum.hungry;
            }
        }
    }
}
