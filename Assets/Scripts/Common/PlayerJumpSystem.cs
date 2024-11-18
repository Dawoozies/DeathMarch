using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerJumpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (jumpVelocity, jumpInput, jumpAirTime, jumpAirTimeMax, jumpState) in SystemAPI.Query<
        RefRW<JumpVelocity>, 
        PlayerJumpInput, 
        JumpAirTime, 
        JumpAirTimeMax,
        RefRW<JumpState>
        >().WithAll<Simulate>())
        {
            if(jumpInput.Value && jumpState.ValueRO.Value == 0)
            {
                Debug.Log($"Jump input happening {jumpInput.Value}");
                jumpState.ValueRW.Value = 1;
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