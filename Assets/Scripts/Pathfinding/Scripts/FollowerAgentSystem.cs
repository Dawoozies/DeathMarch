using Pathfinding.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct FollowerAgentSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DestinationTag>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var destinationEntity = SystemAPI.GetSingletonEntity<DestinationTag>();
        NativeList<LocalTransform> destinationTransforms = new NativeList<LocalTransform>(Allocator.Temp);
        foreach (var localTransform in SystemAPI.Query<LocalTransform>().WithAll<Simulate, DestinationTag>())
        {
            destinationTransforms.Add(localTransform);
        }
        if(destinationTransforms.Length <= 0)
            return;
        foreach (var (destinationPoint, transform) in SystemAPI.Query<RefRW<DestinationPoint>, LocalTransform>().WithAll<Simulate>())
        {
            destinationPoint.ValueRW.destination = GetClosestDestination(destinationTransforms, transform.Position).Position;
        }
    }
    public LocalTransform GetClosestDestination(NativeList<LocalTransform> destinations, float3 agentPos)
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