using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerJumpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (jumpVelocity, jumpInput, jumpState, jumpStrength, groundCheck) in SystemAPI.Query<
        RefRW<JumpVelocity>,
        PlayerJumpInput,
        RefRW<JumpState>,
        JumpStrength,
        GroundCheck
        >().WithAll<Simulate>())
        {
            if(groundCheck.AirTime < jumpState.ValueRO.CoyoteTime && jumpInput.Value && jumpState.ValueRO.State == 0)
            {
                //Debug.Log($"Jump input happening {jumpInput.Value}");
                jumpState.ValueRW.State = 1;
                jumpState.ValueRW.AirTime = jumpState.ValueRO.AirTimeMax;
            }
            if(jumpState.ValueRO.State == 1)
            {
                //if(!jumpInput.Value && jumpState.ValueRO.AirTime > 0)
                //{
                //    jumpState.ValueRW.AirTime = 0;
                //}
                jumpVelocity.ValueRW.Value = math.up() * math.lerp(0, jumpStrength.Value, math.clamp(jumpState.ValueRO.AirTime,0,jumpState.ValueRO.AirTimeMax)/jumpState.ValueRO.AirTimeMax);
                if(jumpState.ValueRO.AirTime > 0)
                {
                    jumpState.ValueRW.AirTime -= SystemAPI.Time.DeltaTime;
                }
                else
                {
                    jumpState.ValueRW.State = 0;
                }
            }
            //if not falling and jump input false
            //0 == can jump
            //1 == jumping and not cancelled
            //2 == jumping and cancelled
            //3 == not cancelled reached maximum air time
            //4 == not cancelled, no max air time, hit ground

            //if jump input true and jumpAirTime < jumpAirTimeMax we can increase

            //if jumpAirTime >= jumpAirTimeMax regardless we have to move back to zero
        }

    }
}