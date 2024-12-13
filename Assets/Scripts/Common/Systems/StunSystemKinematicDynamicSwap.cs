using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Common.Systems
{
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct StunSystemKinematicDynamicSwap : ISystem
    {
        //private float timer;
        //private float maxTime = 0.1f;

        public void OnCreate(ref SystemState state)
        {
            //RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }
        public void OnUpdate(ref SystemState state)
        {
            // timer += SystemAPI.Time.DeltaTime;
            // if (timer < maxTime)
            // {
            //     return;
            // }

            //int maxToProcess = 10;
            //int processedCount = 0;
            //var ecbSingleton = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            //var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);
            //if stunTime > 0 && isKinematic then we gotta process it
            foreach (var (physicsMassOverride,currentHitPoints,entity) in SystemAPI.Query<RefRW<PhysicsMassOverride>, RefRO<CurrentHitPoints>>().WithAll<Simulate>().WithEntityAccess())
            {
                if (physicsMassOverride.ValueRO.IsKinematic == 1 && currentHitPoints.ValueRO.StunTime > 0)
                {
                    physicsMassOverride.ValueRW.IsKinematic = 0;
                    physicsMassOverride.ValueRW.SetVelocityToZero = 0;
                }
                if (currentHitPoints.ValueRO.StunTime <= 0 && physicsMassOverride.ValueRO.IsKinematic == 0)
                {
                    physicsMassOverride.ValueRW.IsKinematic = 1;
                    physicsMassOverride.ValueRW.SetVelocityToZero = 1;
                }
            }
            // timer = 0f;
        }
    }
}