using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class PlayerShootingSystem : SystemBase// SystemBase: other class can reference you and use SystemAPI
{
    public event EventHandler OnShoot;


    protected override void OnCreate()
    {
        RequireForUpdate <Player>();
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<Player>();
            EntityManager.SetComponentEnabled<Stunned>(playerEntity, false);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<Player>();
            EntityManager.SetComponentEnabled<Stunned>(playerEntity, true);
        }
        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }


        //Singleton: one and only one entity with this component
        SpawnCubeConfig spawnCubeConfig = SystemAPI.GetSingleton<SpawnCubeConfig>();


        //EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(WorldUpdateAllocator); //better



        //              WithEntityAccess():         use with this ¡õ
        foreach ((RefRO<LocalTransform> localTransform, Entity entity) in 
            SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Player>().WithDisabled<Stunned>().WithEntityAccess())
        {
            //the code may be broke if the new instantiated entity has Player TAG,
            //better follow the code in PlayerShootingSystem for generating new entity
            //Entity spawnedEntity = EntityManager.Instantiate(spawnCubeConfig.cubePrefabEntity);

            //use this code instead
            Entity spawnedEntity = entityCommandBuffer.Instantiate(spawnCubeConfig.cubePrefabEntity);//record command
            entityCommandBuffer.SetComponent(spawnedEntity, new LocalTransform
            {
                Position = localTransform.ValueRO.Position,
                Rotation = quaternion.identity,
                Scale = 1f,//MUST add (default scale is 0)
            });

            // For test, to make every cube has different movement vector
            entityCommandBuffer.SetComponent(spawnedEntity, new TestMovement
            {
                movementVector = new float3(UnityEngine.Random.Range(-1, 1), 0, UnityEngine.Random.Range(-1, 1)),
            });
            // for test

            //OnShoot?.Invoke(this, EventArgs.Empty);
            // WithEntityAccess(): 
            OnShoot?.Invoke(entity, EventArgs.Empty);// ¡ö use with this    
        }

        entityCommandBuffer.Playback(EntityManager);//execute the recorded commands
    }
}
