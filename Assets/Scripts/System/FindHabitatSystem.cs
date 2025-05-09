using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

[UpdateAfter(typeof(ConfigUpdateSystem))]
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
            //foreach ((RefRO<LocalTransform> habitatLocalTransform, RefRO<HabitatProperty> habitatProperty) in
            //    SystemAPI.Query<RefRO<LocalTransform>, RefRO<HabitatProperty>>())
            foreach (HabitatAspect habitat in SystemAPI.Query<HabitatAspect>())
            {
                // Habitat limitation size of the animals' max size
                // animals' max size should be between the following two values
                float habMinSize = habitat.minSize;
                float habMaxSize = habitat.maxSize;




                // check vacancy
                if(habitat.vacancy <= 0)
                {
                    // this habitat is full
                    continue;
                }



                if (habMinSize < animalMaxSize && animalMaxSize < habMaxSize)
                {
                    // this animal is able to use this habitat
                    // compare the min distance with other habitats

                    float distance = MathHelpers.GetDistance(animalLocalTransform.ValueRO.Position, habitat.position);




                    if (distance < minDistance || minDistance == -1) // if current habitat is nearer or just having init value
                    {
                        // update nearest
                        minDistance = distance;
                        nearestHabitat = habitat.habitatProperty;
                        habitatPosition = habitat.position;

                    }
                }
            }
            // finished looping through all habitats


            // update habitat by result if found
            if (nearestHabitat != null && minDistance != -1)
            {
                // check & update vacancy

                // Release vacancy
                AnimalHabitatInfo animalHabitatInfo = SystemAPI.GetComponent<AnimalHabitatInfo>(animalEntity);
                if (animalHabitatInfo.habitatProperty != null) // if habitat already assigned
                {
                    ReleaseVacancy((HabitatProperty)animalHabitatInfo.habitatProperty);
                }


                // Update vacancy
                bool vacancyIsEnough = OccupyVacancy((HabitatProperty)nearestHabitat);


                if (vacancyIsEnough)
                {
                    ecb.SetComponent(animalEntity, new AnimalHabitatInfo
                    {
                        habitatProperty = nearestHabitat,
                        habitatPosition = habitatPosition,
                    });
                }
            }




            // disable the IsFindingHabitat tag, no matter whether success to find a habitat or not
            ecb.SetComponentEnabled<IsFindingHabitat>(animalEntity, false);
        }


        ecb.Playback(state.EntityManager);//execute the recorded commands
        ecb.Dispose();

    }

    private void ReleaseVacancy(HabitatProperty hb)
    {
        hb.vacancy -= 1;
        if (hb.vacancy > hb.capacity)
        {
            hb.vacancy = hb.capacity;
        }
    }



    private bool OccupyVacancy(HabitatProperty hb) // return true if vacancy is enough
    {
        if (hb.vacancy - 1 >= 0)
        {
            hb.vacancy -= 1;
            return true;
        }
        return false;
    }
}
