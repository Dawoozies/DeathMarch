using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
        uint initSeed = (uint)(SystemAPI.Time.ElapsedTime + 1);
        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        new ProcessSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            rng = new Random(initSeed),
            Ecb = ecb
        }.ScheduleParallel();
    }

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}
[BurstCompile]
public partial struct ProcessSpawnerJob : IJobEntity
{
    public Random rng;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double ElapsedTime;

    // IJobEntity generates a component data query based on the parameters of its `Execute` method.
    // This example queries for all Spawner components and uses `ref` to specify that the operation
    // requires read and write access. Unity processes `Execute` for each entity that matches the
    // component data query.
    private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
    {
        // If the next spawn time has passed.
        if (spawner.NextSpawnTime < ElapsedTime)
        {
            // Spawns a new entity and positions it at the spawner.
            Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
            uint seed = rng.NextUInt() + 1u;
            float3 randomDir = GeneratePoints.RandomPositionInsideUnitSphere(seed);
            float randomRadius = math.lerp(10, 20, rng.NextFloat());
            float3 spawnOffset = float3.zero;
            spawnOffset.x = randomDir.x;
            spawnOffset.z = randomDir.y;
            Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition + spawnOffset * randomRadius));

            // Resets the next spawn time.
            spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
        }
    }
}