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
    }
    protected override void OnStartRunning()
    {
        _inputActions.Enable();
        _inputActions.Player.Aim.performed += OnAimInput;
    }
    protected override void OnStopRunning()
    {
        _inputActions.Player.Aim.performed -= OnAimInput;
        _inputActions.Disable();
    }
    private void OnAimInput(InputAction.CallbackContext callbackContext)
    {
        float inputValue = callbackContext.ReadValue<float>();
        foreach(var playerInput in SystemAPI.Query<RefRW<PlayerAimInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.Value = inputValue > 0 ? true : false;
        }
    }
    protected override void OnUpdate()
    {
    }
}
