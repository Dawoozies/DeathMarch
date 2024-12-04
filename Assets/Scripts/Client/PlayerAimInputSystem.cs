using Unity.Entities;
using Unity.NetCode;
using UnityEngine.InputSystem;
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerAimInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
        RequireForUpdate<OwnerChampTag>();
        RequireForUpdate<PlayerAimInput>();
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
        RefRO<PlayerAimInput> aimInput = SystemAPI.GetComponentRO<PlayerAimInput>(playerEntity);
        float currentHeldTime = aimInput.ValueRO.HeldTime;
        float nextHeldTime = _inputActions.Player.Aim.IsPressed() ? currentHeldTime + SystemAPI.Time.DeltaTime : 0;
        PlayerAimInput nextAimInput = new PlayerAimInput
        {
            Value = _inputActions.Player.Aim.IsPressed(),
            HeldTime = nextHeldTime
        };
        EntityManager.SetComponentData(playerEntity, nextAimInput);
    }
}
