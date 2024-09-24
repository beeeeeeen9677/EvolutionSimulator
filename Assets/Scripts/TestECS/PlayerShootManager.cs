using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlayerShootManager : MonoBehaviour
{
    [SerializeField]
    private GameObject popUpPrefab;

    private void Start()
    {
        PlayerShootingSystem playerShootingSystem = 
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerShootingSystem>();// must be singleton

        playerShootingSystem.OnShoot += PlayerShootingSystem_OnShoot;
    }

    private void PlayerShootingSystem_OnShoot(object sender, EventArgs e)
    {
        Entity playerEntity = (Entity)sender;
        LocalTransform localTransform = 
            World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(playerEntity);

        Instantiate(popUpPrefab, localTransform.Position, Quaternion.identity);
    }
}
