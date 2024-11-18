using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct VelocitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (physicsVelocity, vMove, vJump, vGravity) in SystemAPI.Query<
        RefRW<PhysicsVelocity>,
        MoveVelocity,
        JumpVelocity,
        GravityVelocity
        >().WithAll<Simulate>())
        {
            float3 finalVelocity = float3.zero;
            finalVelocity += vMove.Value;
            finalVelocity += vJump.Value;
            finalVelocity += vGravity.Value;
            physicsVelocity.ValueRW.Linear = finalVelocity;
        }
    }
}