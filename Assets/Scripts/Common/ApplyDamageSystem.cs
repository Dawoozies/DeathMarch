using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(CalculateFrameDamageSystem))]
public partial struct ApplyDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NetworkTick currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (currentHitPoints, entity) in SystemAPI
            .Query<RefRW<CurrentHitPoints>>()
            .WithAll<Simulate>()
            .WithEntityAccess())
        {
            if(currentHitPoints.ValueRO.Value <= 0)
            {
                ecb.AddComponent<DestroyEntityTag>(entity);
                //put on something else here if
            }
        }
        ecb.Playback(state.EntityManager);
    }
}