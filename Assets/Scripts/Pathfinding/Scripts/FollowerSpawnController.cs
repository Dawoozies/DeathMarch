using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using TMPro;
using Unity.Collections;
using Unity.Transforms;
using Pathfinding.ECS;

public class FollowerSpawnController : MonoBehaviour
{
    public TMP_InputField SpawnCountInput;
    public TextMeshProUGUI StatusText;

    int m_totalSpawnCount = 0;
    public int m_columCount = 200;

    public void PerformClear()
    {
        EntityManager em = EntityUtils.GetDefaultWorldManager();
        EntityQuery query = em.CreateEntityQuery(ComponentType.ReadOnly<WaypointIndexData>());
        em.DestroyEntity(query);
        Pathfinding.RVO.SimulatorBurst sim = UnityEngine.GameObject.Find("RvoSimulator").GetComponent<Pathfinding.RVO.RVOSimulator>().GetSimulator();
        sim.ClearAgents();
        m_totalSpawnCount = 0;
        StatusText.text = $"0";
    }

    public void PerformSpawn()
    {
        EntityManager em = EntityUtils.GetDefaultWorldManager();
        Entity spawnerEntity = EntityUtils.QueryDefaultWorldSingletonEntity<AgentSpawnerOpts>();
        AgentSpawnerOpts spawnerOpts = em.GetComponentData<AgentSpawnerOpts>(spawnerEntity);
        LocalToWorld spawnerLtw = em.GetComponentData<LocalToWorld>(spawnerEntity);
        int spawnCount = int.Parse(SpawnCountInput.text);
        NativeArray<Entity> entities = em.Instantiate(spawnerOpts.PrefabEntity, spawnCount, Allocator.Temp);
        Entity destEntity = EntityUtils.QuerySingletonEntity<DestinationTag>(em);
        LocalToWorld ltwDest = em.GetComponentData<LocalToWorld>(destEntity);

        LocalTransform lt = new LocalTransform();
        lt.Rotation = Quaternion.identity;
        lt.Scale = 1f;

        float3 pos = spawnerLtw.Position;
        float spacing = 1f;

        for (int i = 0; i < spawnCount; i++) {
            Entity spawnedEntity = entities[i];

            // set position
            if ((i % m_columCount) == 0) {
                pos.z += spacing;
                pos.x = spawnerLtw.Position.x;
            }
            pos.x += spacing;
            lt.Position = pos;
            em.SetComponentData(spawnedEntity, lt);

            // set the destination point for the entity
            DestinationPoint destPt = em.GetComponentData<DestinationPoint>(spawnedEntity);
            destPt.destination = ltwDest.Position;
            em.SetComponentData(spawnedEntity, destPt);
        }

        m_totalSpawnCount += spawnCount;
        StatusText.text = $"{m_totalSpawnCount}";

        entities.Dispose();
    }

    //public void PerformSpawn()
    //{
    //    EntityManager em = EntityUtils.GetDefaultWorldManager();

    //    Entity spawnerEntity = EntityUtils.QueryDefaultWorldSingletonEntity<AgentSpawnerOpts>();

    //    AgentSpawnerOpts spawnerOpts = em.GetComponentData<AgentSpawnerOpts>(spawnerEntity);
    //    LocalToWorld spawnerLtw = em.GetComponentData<LocalToWorld>(spawnerEntity);

    //    int spawnCount = int.Parse(SpawnCountInput.text);

    //    // instantiate all entities at the same time
    //    NativeArray<Entity> entities = em.Instantiate(spawnerOpts.PrefabEntity, spawnCount, Allocator.Temp);

    //    float3 pos = spawnerLtw.Position;

    //    float spacing = 1f;

    //    Entity destEntity = EntityUtils.QuerySingletonEntity<DestinationTag>(em);
    //    LocalToWorld ltwDest = em.GetComponentData<LocalToWorld>(destEntity);

    //    //// blocks until finished. (shared path)
    //    //Pathfinding.Path sharedPath = PathManager.CalculateAbPath(entities[0], pos, ltwDest.Position, new RadiusData { heightRadius = 1f, xzRadius = 1f });
    //    //PathManager.ApplyDefaultStaticMods(sharedPath);

    //    //blocks until finished(tracer path)
    //    Pathfinding.FloodPath floodPath = PathManager.Instance.CalculateFloodPath(ltwDest.Position);

    //    LocalTransform lt = new LocalTransform();
    //    lt.Rotation = Quaternion.identity;
    //    lt.Scale = 1f;

    //    for (int i = 0; i < spawnCount; i++) {
    //        Entity spawnedEntity = entities[i];

    //        // set position
    //        if ((i % m_columCount) == 0) { 
    //            pos.z += spacing;
    //            pos.x = spawnerLtw.Position.x;
    //        }
    //        pos.x += spacing;
    //        lt.Position = pos;
    //        em.SetComponentData(spawnedEntity, lt);

    //        //// set shared path
    //        //if (i != 0) { PathUtils.CopyPathAndReset(em, sharedPath.vectorPath, entities[i]); }

    //        //// set unique path
    //        //PathManager.Instance.StartAbPath(spawnedEntity, pos, ltwDest.Position);

    //        // set tracer path using flood path
    //        PathManager.Instance.StartTracerPath(spawnedEntity, pos, floodPath);
    //    }

    //    m_totalSpawnCount += spawnCount;

    //    StatusText.text = $"{m_totalSpawnCount}";

    //    entities.Dispose();
    //}
}
