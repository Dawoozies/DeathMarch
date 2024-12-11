using Pathfinding.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
//queue distance calculations
public struct QueuedAgentTag : IComponentData { }
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class FollowerAgentSystem : SystemBase
{
    //get everyone who is not in the queue and add them - destination point not in queue
    //first N agents in queue get their destinations set
    //repeat
    private NativeList<Entity> agents;
    private EntityQuery destinationQuery;
    //if i want to do 10 per frame
    private float timer;
    private float maxTime = 0.1f;
    protected override void OnCreate()
    {
        RequireForUpdate<DestinationTag>();
        agents = new NativeList<Entity>(Allocator.Persistent);
        destinationQuery = GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<DestinationTag>());
        timer = maxTime;
    }
    protected override void OnDestroy()
    {
        agents.Dispose();
    }
    [BurstCompile]
    protected override void OnUpdate()
    {
        timer += SystemAPI.Time.DeltaTime;
        if (timer < maxTime)
        {
            return;
        }
        //change this so that it spreads this over many frames
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<DestinationPoint>().WithNone<QueuedAgentTag>().WithAll<Simulate>().WithEntityAccess())
        {
            ecb.AddComponent<QueuedAgentTag>(entity);
            agents.Add(entity);
            //UnityEngine.Debug.LogError($"Adding agent to agents");
        }
        ecb.Playback(EntityManager);
        int maxToProcess = 5;
        int amountToProcess = math.min(maxToProcess, agents.Length);
        if (amountToProcess <= 0)
            return;

        var destinationEntity = SystemAPI.GetSingletonEntity<DestinationTag>();
        var destinations = destinationQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        for (int i = 0; i < amountToProcess; i++)
        {
            if (agents.Length <= 0)
                break;
            Entity agent = agents[0];
            if (!EntityManager.Exists(agent))
            {
                //remove this guy
                agents.RemoveAt(0);
                continue;
            }
            if(!EntityManager.HasComponent<LocalTransform>(agent))
            {
                agents.RemoveAt(0);
                continue;
            }
            if (EntityManager.HasComponent<DestroyEntityTag>(agent))
            {
                agents.RemoveAt(0);
                continue;
            }
            LocalTransform agentTransform = EntityManager.GetComponentData<LocalTransform>(agent);
            EntityManager.SetComponentData<DestinationPoint>(agent, new DestinationPoint
            {
                destination = GetClosestDestination(destinations, agentTransform.Position).Position
            });
            agents.RemoveAt(0);
            agents.Add(agent);
        }
        destinations.Dispose();
        timer = 0f;
        //UnityEngine.Debug.LogError($"Timer={timer} amountToProcess={amountToProcess} agents.Length={agents.Length}");
    }
    public LocalTransform GetClosestDestination(NativeArray<LocalTransform> destinations, float3 agentPos)
    {
        float closestDist = math.INFINITY;
        int closestDestination = 0;
        if (destinations.Length == 1)
        {
            return destinations[closestDestination];
        }
        for (int i = 1; i < destinations.Length; i++)
        {
            float3 pos = destinations[i].Position;
            float d = math.distancesq(agentPos, pos);
            if (d < closestDist)
            {
                closestDist = d;
                closestDestination = i;
            }
        }
        return destinations[closestDestination];
    }
}
//if something has destinationPoint then it needs updating
//this will be on every destination point