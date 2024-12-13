using Unity.Burst;
using Unity.Entities;
namespace Common.Systems
{
    [BurstCompile]
    public partial struct StunSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var currentHitPoints in SystemAPI.Query<RefRW<CurrentHitPoints>>().WithAll<Simulate>())
            {
                if (currentHitPoints.ValueRO.StunTime > 0f)
                {
                    currentHitPoints.ValueRW.StunTime -= SystemAPI.Time.DeltaTime;
                }
            }
        }
    }
}