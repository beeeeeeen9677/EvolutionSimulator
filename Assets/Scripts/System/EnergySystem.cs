using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        /*
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new EnergyHandleJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),

        }.ScheduleParallel();
        */

        
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged); 
        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach (AnimalAspect animal in SystemAPI.Query<AnimalAspect>())
        {
            animal.ConsumeEnergy(deltaTime);
            // dead if no more energy
            if (animal.GetEnergy() <= 0)
            {
                // clear target's threat
                if (animal.IsTargetExist())
                {
                    AnimalAspect targetAnimal = SystemAPI.GetAspect<AnimalAspect>(animal.GetTargetEntity());
                    targetAnimal.ClearThreat(animal.entity);
                    Debug.Log("Threat clear since threat died");
                }



                // destroy itself
                ecb.DestroyEntity(animal.entity);


                //animal.entity = Entity.Null;

                // add ToBeDestroyed Component and System later
            }
        }
        
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
