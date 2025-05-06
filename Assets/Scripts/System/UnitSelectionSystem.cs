using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using RaycastHit = Unity.Physics.RaycastHit;


public partial class UnitSelectionSystem : SystemBase  // SystemBase: other class can reference you and use SystemAPI
{
    private Camera mainCamera;
    private CollisionWorld collisionWorld;
    //private PhysicsWorldSingleton physicsWorldSingleton;
    public Action<Entity> OnAnimalSelected;
    


    protected override void OnCreate()
    {
        mainCamera = Camera.main;
        RequireForUpdate<SelectableUnitTag>();
        //physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
    }

    protected override void OnUpdate()
    {
        // on mouse release
        if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) // Pointer Over UI Element
                return;

            SelectSingleUnit();
        }


       

    }


    // select the mouse clicked entity
    private void SelectSingleUnit()
    {
        if(mainCamera == null)
            mainCamera = Camera.main;

        collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        //collisionWorld = physicsWorldSingleton.CollisionWorld;

        UnityEngine.Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);


        float rayDistance = 500f;
        Vector3 rayStart = ray.origin;
        Vector3 rayEnd = ray.GetPoint(rayDistance);

        // create mouse raycast
        if (MouseRaycast(rayStart, rayEnd, out RaycastHit raycastHit))
        {
            Entity hitEntity = SystemAPI.GetSingleton<PhysicsWorldSingleton>().Bodies[raycastHit.RigidBodyIndex].Entity;
            //Entity hitEntity = physicsWorldSingleton.Bodies[raycastHit.RigidBodyIndex].Entity;


            // if this entity is Selectable
            if (SystemAPI.HasComponent<SelectableUnitTag>(hitEntity))
            {
                Debug.Log("Clicked: " + hitEntity.Index);
                //SystemAPI.SetComponentEnabled<IsSelectedTag>(hitEntity, true);

                OnAnimalSelected?.Invoke(hitEntity);
            }
        }
    }



    // RaycastHit is under Unity.Physics for DOTS
    private bool MouseRaycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit )
    {
        RaycastInput raycastInput = new RaycastInput
        {
            Start = rayStart,
            End = rayEnd,
            Filter = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.SelectionLayer,
                //CollidesWith = (uint)(CollisionLayer.Animal | CollisionLayer.Ground),
                CollidesWith = (uint)CollisionLayer.Animal,
            }
        };


        return collisionWorld.CastRay(raycastInput, out raycastHit);
    }

    public void ToggleSystem(bool value)
    {
        Enabled = value;
    }
}
