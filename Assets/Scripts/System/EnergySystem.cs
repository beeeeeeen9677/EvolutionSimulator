using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public partial struct EnergySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Energy>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();



        new EnergyHandleJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),

        }.ScheduleParallel();
    }
}

public partial struct EnergyHandleJob : IJobEntity
{
    public float deltaTime;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(AnimalAspect animal, [EntityIndexInQuery] int sortKey)
    {
        animal.ConsumeEnergy(deltaTime);

        // dead if no more energy
        if (animal.GetEnergy() <= 0)
        {
            ecb.DestroyEntity(sortKey, animal.entity);
            //animal.entity = Entity.Null;

            // add ToBeDestroyed Component and System later
        }
    }
}
