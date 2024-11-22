using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerShootInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
        RequireForUpdate<OwnerChampTag>();
        RequireForUpdate<PlayerShootInput>();
    }
    protected override void OnStartRunning()
    {
        _inputActions.Enable();
    }
    protected override void OnStopRunning()
    {
        _inputActions.Disable();
    }
    protected override void OnUpdate()
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
        float currentHeldTime = SystemAPI.GetComponentRO<PlayerShootInput>(playerEntity).ValueRO.HeldTime;
        EntityManager.SetComponentData(playerEntity, new PlayerShootInput
        {
            HeldTime = _inputActions.Player.Shoot.IsPressed() ? currentHeldTime + SystemAPI.Time.DeltaTime : 0
        });
        Debug.Log($"CurrentHeldTime = {currentHeldTime}");
    }
}
