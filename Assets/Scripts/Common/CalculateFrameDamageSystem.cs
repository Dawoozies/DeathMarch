using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct CalculateFrameDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NetworkTick currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        foreach (var (damageBuffer, damageThisTickBuffer) in SystemAPI
            .Query<DynamicBuffer<DamageBufferElement>, DynamicBuffer<DamageThisTick>>()
            .WithAll<Simulate>())
        {
            if(damageBuffer.IsEmpty)
            {
                damageThisTickBuffer.AddCommandData(new DamageThisTick
                {
                    Tick = currentTick,
                    Value = 0
                });
            }
            else
            {
                int totalDamage = 0;
                if(damageThisTickBuffer.GetDataAtTick(currentTick, out var damageThisTick))
                {
                    //we do this bc multiple client frames
                    totalDamage = damageThisTick.Value;
                }
                foreach (var damage in damageBuffer)
                {
                    totalDamage += damage.Value;
                }
                damageThisTickBuffer.AddCommandData(new DamageThisTick
                {
                    Tick = currentTick, 
                    Value = totalDamage
                });
                damageBuffer.Clear();
            }
        }
    }
}