using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BatchSettingAuthoring : MonoBehaviour
{
    [SerializeField]
    private int BatchSize; // number of animals to be processed in each batch


    public class BatchSettingBaker : Baker<BatchSettingAuthoring>
    {
        public override void Bake(BatchSettingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AnimalBatch
            {
                BatchSize = authoring.BatchSize,
                CurrentBatchIndex = 0,
                CycleCount = 0,
            });
        }
    }
}

public struct AnimalBatch : IComponentData
{
    public int BatchSize; // number of animals to be processed in each batch
    public int CurrentBatchIndex;
    public int CycleCount;
}