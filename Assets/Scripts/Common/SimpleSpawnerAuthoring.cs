using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class SimpleSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public bool respawn;
    public class ComponentBaker : Baker<SimpleSpawnerAuthoring>
    {
        public override void Bake(SimpleSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SimpleSpawner
            {
                PrefabToSpawn = GetEntity(authoring.prefabToSpawn,TransformUsageFlags.Dynamic),
                SpawnPosition = authoring.transform.position,
                Respawn = authoring.respawn,
                ShouldSpawn = true
            });
        }
    }
}
//id have to (on spawning) set the component
//destroyentitytag
public struct SimpleSpawner : IComponentData
{
    public Entity PrefabToSpawn;
    public float3 SpawnPosition;
    public bool Respawn;
    public bool ShouldSpawn;
    public Entity SpawnedEntity;
}