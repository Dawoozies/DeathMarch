using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerJumpInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
        //RequireForUpdate<OwnerChampTag>();
    }
    protected override void OnStartRunning()
    {
        _inputActions.Enable();
        _inputActions.Player.Jump.performed += OnJumpInput;
    }
    protected override void OnStopRunning()
    {
        _inputActions.Player.Jump.performed -= OnJumpInput;
        _inputActions.Disable();
    }
    private void OnJumpInput(InputAction.CallbackContext callbackContext)
    {
        float inputValue = callbackContext.ReadValue<float>();
        foreach(var (playerInput, entity) in SystemAPI.Query<RefRW<PlayerJumpInput>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            playerInput.ValueRW.Value = inputValue > 0 ? true : false;
        }
        //Debug.Log($"Jump Input = {inputValue}");
        //Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
        // EntityManager.SetComponentData(playerEntity, new PlayerJumpInput
        // {
        //     Value = inputValue > 0 ? true : false
        // });
    }
    protected override void OnUpdate()
    {
    }
}
