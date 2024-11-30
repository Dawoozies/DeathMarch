using Unity.Entities;
using UnityEngine;

public struct AgentSpawnerOpts : IComponentData
{
    public Entity PrefabEntity;
}


public class AgentGridSpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
}


public class AgentGridSpawnerBaker : Baker<AgentGridSpawnerAuthoring>
{
    public override void Bake(AgentGridSpawnerAuthoring authoring)
    {
        Entity prefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
        Entity spawnerEntity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(spawnerEntity, new AgentSpawnerOpts { PrefabEntity = prefabEntity });
    }
}