using Unity.Entities;

[UpdateAfter(typeof(ConfigUpdateSystem))]
public partial struct GrassPropertiesSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GrassProperties>();
        //state.RequireForUpdate<GrassGrowUpSystemFlag>();
    }


    public void OnUpdate(ref SystemState state)
    {
        /*
        var grassFlag = SystemAPI.GetSingletonRW<GrassGrowUpSystemFlag>();



        if (grassFlag.ValueRO.flag == false)
            return;

        //Debug.Log("Grass Grow UP: " + grassFlag.ValueRO.flag);

        grassFlag.ValueRW.flag = false;

        foreach (var grass in SystemAPI.Query<GrassAspect>())
        {
            grass.GrowUp(SystemAPI.Time.DeltaTime);
        }
        */


        new GrassPropertiesJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();

    }
}


public partial struct GrassPropertiesJob : IJobEntity
{
    public float deltaTime;
    public void Execute(GrassAspect grass)
    {
        grass.GrowUp(deltaTime);
        //Debug.Log(grass._localTransform.ValueRO.Position);
    }
}

public struct GrassGrowUpSystemFlag : IComponentData
{
    public bool flag; // set to true to call update function (grow up)
}