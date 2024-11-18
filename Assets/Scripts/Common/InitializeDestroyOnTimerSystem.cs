using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
public partial struct InitializeDestroyOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        int simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        NetworkTick currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        foreach (var (destroyOnTimer, entity) in SystemAPI.Query<DestroyOnTimer>().WithNone<DestroyAtTick>().WithEntityAccess())
        {
            uint lifetimeInTicks = (uint)(destroyOnTimer.Value * simulationTickRate);
            NetworkTick targetTick = currentTick;
            targetTick.Add(lifetimeInTicks);
            ecb.AddComponent(entity, new DestroyAtTick{Value = targetTick});
        }
        ecb.Playback(state.EntityManager);
    }
}