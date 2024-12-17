using Pathfinding.ECS;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Common.Systems
{
    public partial class StunRecoverySystem : SystemBase
    {
        private float maxRecoveryNavMeshDistSq = 25;
        private int processPerFrameCount = 10;
        protected override void OnUpdate()
        {
            int toProcessCount = processPerFrameCount;
            foreach (var (physicsMassOverride, movementState,currentHitPoints, localTransform,entity) in SystemAPI.Query<
                         RefRW<PhysicsMassOverride>,
                         MovementState,
                         RefRO<CurrentHitPoints>,
                         LocalTransform
                     >().WithAll<Simulate>().WithEntityAccess())
            {
                if (toProcessCount <= 0)
                    break;
                if (currentHitPoints.ValueRO.StunTime <= 0 && physicsMassOverride.ValueRO.IsKinematic == 0)
                {
                    //we need to do distance check between boi and the navmesh
                    if (math.distancesq(localTransform.Position, movementState.closestOnNavmesh) <=
                        maxRecoveryNavMeshDistSq)
                    {
                        //then recover
                        physicsMassOverride.ValueRW.IsKinematic = 1;
                        physicsMassOverride.ValueRW.SetVelocityToZero = 1;
                    }

                    toProcessCount--;
                }
            }            
        }
    }
}