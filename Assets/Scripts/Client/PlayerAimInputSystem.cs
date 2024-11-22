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
        Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
        EntityManager.SetComponentData(playerEntity, new PlayerAimInput
        {
            Value = inputValue > 0 ? true : false
        });
    }
    protected override void OnUpdate()
    {
    }
}
