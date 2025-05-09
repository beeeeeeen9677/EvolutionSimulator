using Unity.Entities;
using UnityEngine;

public class InitLakeAuthoring : MonoBehaviour
{
    public GameObject lakePrefab;
    public int initLakeNumber;
    public int fieldSize;


    public class InitLakeConfigBaker : Baker<InitLakeAuthoring>
    {
        public override void Bake(InitLakeAuthoring authoring)
        {
            /*
            int initLakeNumber = PlayerPrefs.GetInt("InitLakeNum", authoring.initLakeNumber);
            PlayerPrefs.SetInt("InitLakeNum", initLakeNumber);
            */


            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitLakeConfig
            {
                lakePrefab = GetEntity(authoring.lakePrefab, TransformUsageFlags.None),
                initLakeNumber = authoring.initLakeNumber,
                fieldSize = authoring.fieldSize,
            });
        }
    }
}

public struct InitLakeConfig : IComponentData
{
    public Entity lakePrefab;
    public int initLakeNumber; // number to spawn initially
    public int fieldSize;

}