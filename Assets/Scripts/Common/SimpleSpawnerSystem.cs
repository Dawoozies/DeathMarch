using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct SimpleSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var spawner in SystemAPI.Query<RefRW<SimpleSpawner>>().WithAll<Simulate>())
        {
            if(spawner.ValueRO.ShouldSpawn)
            {
                Entity spawnedEntity = state.EntityManager.Instantiate(spawner.ValueRO.PrefabToSpawn);
                state.EntityManager.SetComponentData(spawnedEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition));
                spawner.ValueRW.ShouldSpawn = false;
                spawner.ValueRW.SpawnedEntity = spawnedEntity;
            }

            if(spawner.ValueRO.Respawn && !spawner.ValueRO.ShouldSpawn)
            {
                var entityExists = state.EntityManager.UniversalQuery.GetEntityQueryMask();
                if(!entityExists.MatchesIgnoreFilter(spawner.ValueRO.SpawnedEntity))
                {
                    spawner.ValueRW.ShouldSpawn = true;
                }
            }
        }
    }
    public void OnDestroy(ref SystemState state)
    {
    }
}
// public partial struct ProcessSimpleSpawnJob : IJobEntity
// {
//     public EntityCommandBuffer.ParallelWriter Ecb;
//     private void Execute([ChunkIndexInQuery]int chunkIndex, ref SimpleSpawner spawner)
//     {
//         if(spawner.ShouldSpawn)
//         {
//             Entity spawnedEntity = Ecb.Instantiate(chunkIndex, spawner.PrefabToSpawn);
//             Ecb.SetComponent(chunkIndex, spawnedEntity, LocalTransform.FromPosition(spawner.SpawnPosition));
//             spawner.ShouldSpawn = false;
//             spawner.SpawnedEntity = spawnedEntity;
//         }
//         else
//         {
//             if(spawner.SpawnedEntity.Index >= 0)
//             {
//             }
//             if(spawner.Respawn)
//             {
//                 //if(spawner.SpawnedEntity.Index < 0 && spawner.EntityNotDeferred)
//                 //{
//                 //    spawner.ShouldSpawn = true;
//                 //    spawner.EntityNotDeferred = false;
//                 //}
//             }
//         }
//     }
// }