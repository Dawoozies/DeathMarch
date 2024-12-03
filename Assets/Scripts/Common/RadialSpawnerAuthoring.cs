using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.Collections.Generic;
public class RadialSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int maxCount;
    public float spawnTime;
    public int batchSize;
    public float startAngle;
    public float radius;
    public float angleDelta;
    public class ComponentBaker : Baker<RadialSpawnerAuthoring>
    {
        public override void Bake(RadialSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RadialSpawner>(entity, new RadialSpawner
            {
                PrefabToSpawn = GetEntity(authoring.prefab,TransformUsageFlags.Dynamic),
                MaxCount = authoring.maxCount,
                BatchSize = authoring.batchSize,
                SpawnTime = authoring.spawnTime,
                CurrentAngle = authoring.startAngle,
                Radius = authoring.radius,
                AngleDelta = authoring.angleDelta,
            });
        }
    }
}
public struct RadialSpawner : IComponentData
{
    public Entity PrefabToSpawn;
    public int MaxCount;
    public int BatchSize;
    public float SpawnTime;
    public float SpawnTimer;
    public float CurrentAngle;
    public float Radius;
    public float AngleDelta;
}