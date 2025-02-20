using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct FindHabitatSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HabitatProperty>();
        state.RequireForUpdate<IsFindingHabitat>();
    }


    public void OnUpdate(ref SystemState state)
    {


        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator); //better


        foreach ((RefRO<LocalTransform> animalLocalTransform, RefRO<SizeProperty> animalSize, Entity animalEntity) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<SizeProperty>>().WithAll<AnimalTag>().WithAll<IsFindingHabitat>().WithEntityAccess())
        {
            //Debug.Log(localTransform.ValueRO.Position);


            float animalMaxSize = animalSize.ValueRO.maxSize;



            // min distance between current animal and different habitats
            float minDistance = -1;
            HabitatProperty? nearestHabitat = null;
            Vector3 habitatPosition = Vector3.zero;


            // loop through all habitat
            foreach ((RefRO<LocalTransform> habitatLocalTransform, RefRO<HabitatProperty> habitatProperty) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<HabitatProperty>>())
            {
                // Habitat limitation size of the animals' max size
                // animals' max size should be between the following two values
                float habMinSize = habitatProperty.ValueRO.minSize;
                float habMaxSize = habitatProperty.ValueRO.maxSize;




                if (habMinSize < animalMaxSize && animalMaxSize < habMaxSize)
                {
                    // this animal is able to use this habitat
                    // compare the min distance with other habitats

                    float distance = MathHelpers.GetDistance(animalLocalTransform.ValueRO.Position, habitatLocalTransform.ValueRO.Position);


                    if (distance < minDistance || minDistance == -1) // if current habitat is nearer or just having init value
                    {
                        // update nearest
                        minDistance = distance;
                        nearestHabitat = habitatProperty.ValueRO;
                        habitatPosition = habitatLocalTransform.ValueRO.Position;

                    }
                }
            }
            // finished looping through all habitats


            // update habitat by result if found
            if (nearestHabitat != null && minDistance != -1)
            {
                ecb.SetComponent(animalEntity, new AnimalHabitatInfo
                {
                    habitatProperty = nearestHabitat,
                    habitatPosition = habitatPosition,
                });
            }




            // disable the IsFindingHabitat tag, no matter whether success to find a habitat or not
            ecb.SetComponentEnabled<IsFindingHabitat>(animalEntity, false);
        }


        ecb.Playback(state.EntityManager);//execute the recorded commands
        ecb.Dispose();

    }

}
