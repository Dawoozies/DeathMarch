using Pathfinding.ECS;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace Common.Systems
{
    public partial struct StunPathSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (physicsMassOverride,managedState,entity) in SystemAPI.Query<
                         RefRW<PhysicsMassOverride>, 
                         ManagedState
                     >().WithAll<Simulate>().WithEntityAccess())
            {
                if (physicsMassOverride.ValueRO.IsKinematic == 0 && managedState.pathTracer.hasPath)
                {
                    managedState.ClearPath();
                }
            }            
        }
    }
}