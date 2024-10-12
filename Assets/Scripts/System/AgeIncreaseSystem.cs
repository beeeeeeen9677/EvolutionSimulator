using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct AgeIncreaseSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Age>();
    }

    public void OnUpdate(ref SystemState state)
    {
        new IncreaseAgeJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
}


public partial struct IncreaseAgeJob : IJobEntity
{
    public float deltaTime;
    public void Execute(AgeAspect animal)
    {
        animal.IncreaseAge(deltaTime);
    }
}