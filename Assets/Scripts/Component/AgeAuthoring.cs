using Unity.Entities;
using UnityEngine;

public class AgeAuthoring : MonoBehaviour
{
    // age for getting to another stage
    public int matureThreshold;
    public int agingThreshold;



    public class AgeBaker : Baker<AgeAuthoring>
    {
        public override void Bake(AgeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Age
            {
                currentAge = 0
            });
            AddComponent(entity, new AgeStage
            {
                matureThreshold = authoring.matureThreshold,
                agingThreshold = authoring.agingThreshold,

                currentStage = AgeStageEnum.infant, // initial stage
            });
        }
    }

}

public struct Age : IComponentData
{
    public float currentAge;
}
public struct AgeStage : IComponentData
{
    // age for getting to anothor stage
    public int matureThreshold;
    public int agingThreshold;

    public AgeStageEnum currentStage;
}

public enum AgeStageEnum
{
    infant = 0,
    mature = 1,
    aging = 2,
}