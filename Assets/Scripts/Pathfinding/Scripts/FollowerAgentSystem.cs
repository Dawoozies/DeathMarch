using Pathfinding.ECS;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
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
        var destinationTransform = state.EntityManager.GetComponentData<LocalTransform>(destinationEntity);
        foreach (var destinationPoint in SystemAPI.Query<RefRW<DestinationPoint>>().WithAll<Simulate>())
        {
            destinationPoint.ValueRW.destination = destinationTransform.Position; 
        }
    }
}