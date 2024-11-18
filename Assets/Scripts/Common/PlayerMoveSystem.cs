using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (moveVelocity, moveInput, moveSpeed) in SystemAPI.Query<
        RefRW<MoveVelocity>,
        PlayerMoveInput, 
        MoveSpeed
        >().WithAll<Simulate>())
        {
            float3 velocity = float3.zero;
            velocity.x = moveInput.Value.x;
            velocity.z = moveInput.Value.y;
            //v.ValueRW.Linear = velocity * moveSpeed.Value;
            moveVelocity.ValueRW.Value = velocity * moveSpeed.Value;
        }
    }
}