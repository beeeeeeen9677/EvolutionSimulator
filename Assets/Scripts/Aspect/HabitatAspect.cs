using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct HabitatAspect : IAspect
{
    public readonly Entity entity;

    public readonly RefRW<LocalTransform> _localTransform;

    public readonly RefRW<HabitatProperty> _habitatProperty;


    public float3 position
    {
        get => _localTransform.ValueRO.Position;
        private set => _localTransform.ValueRW.Position = value;
    }

    public HabitatProperty habitatProperty
    {
        get => _habitatProperty.ValueRO;
    }

    public float minSize
    {
        get => _habitatProperty.ValueRO.minSize;
        private set => _habitatProperty.ValueRW.minSize = value;
    }

    public float maxSize
    {
        get => _habitatProperty.ValueRO.maxSize;
        private set => _habitatProperty.ValueRW.maxSize = value;
    }

    public float radius
    {
        get => _habitatProperty.ValueRO.radius;
        private set => _habitatProperty.ValueRW.radius = value;
    }
    public float effectOfHiding
    {
        get => _habitatProperty.ValueRO.effectOfHiding;
        private set => _habitatProperty.ValueRW.effectOfHiding = value;
    }
    public int vacancy
    {
        get => _habitatProperty.ValueRO.vacancy;
        private set => _habitatProperty.ValueRW.vacancy = value;
    }
    public int capacity
    {
        get => _habitatProperty.ValueRO.capacity;
        private set => _habitatProperty.ValueRW.capacity = value;
    }

    public bool OccupyVacancy(int number)
    {
        if (vacancy - number >= 0)
        {
            vacancy -= number;
            return true;
        }
        return false;
    }

    public void ReleaseVacancy(int number)
    {
        vacancy += number;
        if(vacancy > capacity)
        {
            vacancy = capacity;
        }
    }
}