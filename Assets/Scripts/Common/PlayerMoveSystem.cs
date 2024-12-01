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
        foreach (var (moveVelocity, moveInput, moveSpeed, cameraDirections) in SystemAPI.Query<
        RefRW<MoveVelocity>,
        PlayerMoveInput, 
        MoveSpeed,
        PlayerCameraDirections
        >().WithAll<Simulate>())
        {
            float3 velocity = float3.zero;
            //velocity.x = moveInput.Value.x;
            //velocity.z = moveInput.Value.y;
            //velocity.x = moveInput.Value.x * cameraDirections.Right.x + moveInput.Value.y * cameraDirections.Forward.x;
            //velocity.z = moveInput.Value.x * cameraDirections.Right.y + moveInput.Value.y * cameraDirections.Forward.y;
            velocity += moveInput.Value.x * cameraDirections.Right;
            velocity += moveInput.Value.y * cameraDirections.Forward;
            //zero out the y components then normalise
            velocity.y = 0;
            velocity = math.normalizesafe(velocity);

            // n = moveInput.x * camRight + moveInput.y * camForward
            //v.ValueRW.Linear = velocity * moveSpeed.Value;
            moveVelocity.ValueRW.Value = velocity * moveSpeed.Value;
        }
    }
}