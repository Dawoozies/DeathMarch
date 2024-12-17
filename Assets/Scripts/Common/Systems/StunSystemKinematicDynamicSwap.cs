using Pathfinding.ECS;
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
        public void OnCreate(ref SystemState state)
        {
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (physicsMassOverride,currentHitPoints,entity) in SystemAPI.Query<
                         RefRW<PhysicsMassOverride>,
                         RefRO<CurrentHitPoints>
                     >().WithAll<Simulate>().WithEntityAccess())
            {
                if (physicsMassOverride.ValueRO.IsKinematic == 1 && currentHitPoints.ValueRO.StunTime > 0)
                {
                    physicsMassOverride.ValueRW.IsKinematic = 0;
                    physicsMassOverride.ValueRW.SetVelocityToZero = 0;
                }
            }
        }
    }
}