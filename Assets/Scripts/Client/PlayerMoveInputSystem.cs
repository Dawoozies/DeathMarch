using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerMoveInputSystem : SystemBase
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
        _inputActions.Player.Move.performed += OnMovePerformed;
    }
    protected override void OnStopRunning()
    {
        _inputActions.Player.Move.performed -= OnMovePerformed;
        _inputActions.Disable();
    }
    private void OnMovePerformed(InputAction.CallbackContext callbackContext)
    {
        float2 moveInput = callbackContext.ReadValue<Vector2>();
        Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();

        EntityManager.SetComponentData(playerEntity, new PlayerMoveInput
        {
           Value = moveInput
        });
    }
    protected override void OnUpdate()
    {
    }
}