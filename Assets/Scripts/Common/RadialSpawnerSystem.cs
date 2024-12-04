using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Pathfinding.ECS;
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct RadialSpawnerSystem : ISystem
{
    const int maximumSpawnAmount = 30;
    private EntityQuery destinationPointQuery;
    public void OnCreate(ref SystemState state)
    {
        destinationPointQuery = state.GetEntityQuery(ComponentType.ReadOnly<DestinationPoint>());
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawnedEnemies = destinationPointQuery.ToEntityArray(Allocator.Temp);
        if (spawnedEnemies.Length > maximumSpawnAmount)
        {
            return;
        }

        foreach (var (spawner, localTransform) in SystemAPI.Query<RefRW<RadialSpawner>, LocalTransform>().WithAll<Simulate>())
        {
            if (spawner.ValueRO.SpawnTimer < spawner.ValueRO.SpawnTime)
            {
                spawner.ValueRW.SpawnTimer += SystemAPI.Time.DeltaTime;
                continue;
            }
            float currentAngle = spawner.ValueRO.CurrentAngle;
            for (int i = 0; i < spawner.ValueRO.BatchSize; i++)
            {
                Entity spawnedEntity = state.EntityManager.Instantiate(spawner.ValueRO.PrefabToSpawn);
                float3 spawnPos = GetRadialPoint(currentAngle, spawner.ValueRO.Radius);
                spawnPos.y = localTransform.Position.y;
                state.EntityManager.SetComponentData(spawnedEntity, LocalTransform.FromPosition(spawnPos));
                currentAngle = (currentAngle + spawner.ValueRO.AngleDelta) % 360f;
            }
            spawner.ValueRW.CurrentAngle = currentAngle;

            spawner.ValueRW.SpawnTimer = 0f;
        }
    }
    public float3 GetRadialPoint(float currentAngle, float radius)
    {
        float theta = math.radians(currentAngle);
        return new float3(math.cos(theta), 0f, math.sin(theta)) * radius;
    }
    public void OnDestroy(ref SystemState state)
    {
    }
}