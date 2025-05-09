using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;


[UpdateAfter(typeof(ConfigUpdateSystem))]
partial struct InitColorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NeedsColorInitialization>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator); //better

        foreach (var (init, entity) in SystemAPI.Query<NeedsColorInitialization>().WithEntityAccess())
        {
            if (state.EntityManager.HasBuffer<LinkedEntityGroup>(entity))
            {
                var linkedEntityGroupBuffer = state.EntityManager.GetBuffer<LinkedEntityGroup>(entity);
                Entity childEntity = linkedEntityGroupBuffer[1].Value;

                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(childEntity))
                {
                    float4 herbivorousColor = new float4(1.0f, 0.8f, 0.0f, 1.0f);
                    float4 carnivorousColor = new float4(1.0f, 0.3f, 0.3f, 1.0f);
                    float4 medianColor = new float4(0.76f, 1.0f, 1.0f, 1.0f);

                    float4 newAnimalColor;
                    if (init.grassProb > 0.5f)
                    {
                        newAnimalColor = math.lerp(medianColor, herbivorousColor, (init.grassProb - 0.5f) * 2);
                    }
                    else
                    {
                        newAnimalColor = math.lerp(carnivorousColor, medianColor, init.grassProb * 2);
                    }

                    var baseColor = state.EntityManager.GetComponentData<URPMaterialPropertyBaseColor>(childEntity);
                    baseColor.Value = newAnimalColor;
                    ecb.SetComponent(childEntity, baseColor);
                }
            }

            ecb.RemoveComponent<NeedsColorInitialization>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

}
