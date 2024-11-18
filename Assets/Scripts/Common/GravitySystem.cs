using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.NetCode;
//we HAVE to run after we do the ground check
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct GravitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (gravityVelocity, gravity, groundCheck, jumpState) in SystemAPI.Query<
        RefRW<GravityVelocity>,
        RefRW<Gravity>,
        GroundCheck,
        JumpState
        >().WithAll<Simulate>())
        {
            bool nullifyGravity =
                groundCheck.AirTime <= 0f //if we are touching ground
                || jumpState.State == 1 //if we are in jump ascent
                ;
            if(nullifyGravity)
            {
                gravity.ValueRW.AirTime = 0f;
                gravityVelocity.ValueRW.Value = float3.zero;
                continue;
            }

            gravity.ValueRW.AirTime += SystemAPI.Time.DeltaTime;
            float3 dv = gravity.ValueRO.Direction * math.lerp(0, gravity.ValueRO.Strength, math.clamp(gravity.ValueRO.AirTime, 0, gravity.ValueRO.AirTimeMax) / gravity.ValueRO.AirTimeMax) * SystemAPI.Time.DeltaTime;
            float3 v = gravityVelocity.ValueRO.Value + dv;
            float vMagnitude = math.length(v);
            if (vMagnitude > gravity.ValueRO.StrengthMax)
            {
                gravityVelocity.ValueRW.Value = gravity.ValueRO.Direction * gravity.ValueRO.StrengthMax;
                UnityEngine.Debug.Log("HIT MAX GRAVITY VALUE");
            }
            else
            {
                gravityVelocity.ValueRW.Value = v;
            }
            UnityEngine.Debug.Log($"||gravityVelocity||={math.length(gravityVelocity.ValueRO.Value)}");
        }

    }
}