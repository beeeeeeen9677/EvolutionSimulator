using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public partial struct SphereControlSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SphereComponent>();
    }



    float xSpeed;
    float zSpeed;
    private void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;


        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);

        foreach (Entity entity in entities)
        {
            //if this entity has specific component
            if (entityManager.HasComponent<SphereComponent>(entity))
            {
                SphereComponent sphereComponent = entityManager.GetComponentData<SphereComponent>(entity);

                RefRW<PhysicsVelocity> physicsVelocity = SystemAPI.GetComponentRW<PhysicsVelocity>(entity);

                xSpeed = Input.GetAxis("Horizontal") * sphereComponent.moveSpeed * SystemAPI.Time.DeltaTime;
                zSpeed = Input.GetAxis("Vertical") * sphereComponent.moveSpeed * SystemAPI.Time.DeltaTime;

                physicsVelocity.ValueRW.Linear += new float3(xSpeed, 0, zSpeed);


            }
        }

        entities.Dispose();
    }
}
