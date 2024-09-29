using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct GrassPropertiesSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GrassProperties>();
    }

    public void OnUpdate(ref SystemState state)
    {
        new GrassPropertiesJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();
    }
}


public partial struct GrassPropertiesJob : IJobEntity
{
    public float deltaTime;
    public void Execute(GrassAspect grass)
    {
        grass.GrowUp(deltaTime);
        //Debug.Log(grass._localTransform.ValueRO.Position);
    }
}
