using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.NetCode;
using Unity.Physics;
using Unity.Collections;
//we HAVE to run after we do the ground check
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct GravitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (gravityVelocity, gravityDirection, gravityStrength, groundCheck) in SystemAPI.Query<
        RefRW<GravityVelocity>,
        GravityDirection,
        GravityStrength,
        GroundCheck
        >().WithAll<Simulate>())
        {
            if(!groundCheck.isGrounded)
            {
                gravityVelocity.ValueRW.Value = gravityDirection.Value * gravityStrength.Value;
            }
            else
            {
                gravityVelocity.ValueRW.Value = float3.zero;
            }
        }

    }
}