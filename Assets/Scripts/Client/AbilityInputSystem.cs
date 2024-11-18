using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
public partial class AbilityInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
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
        AbilityInput newAbilityInput = new AbilityInput();
        if(_inputActions.GameplayMap.AoeAbility.WasPressedThisFrame())
        {
            //this is how we say the abilityInput is set for this server tick
            newAbilityInput.AoeAbility.Set();
        }
        foreach (var abilityInput in SystemAPI.Query<RefRW<AbilityInput>>())
        {
            abilityInput.ValueRW = newAbilityInput;
        }
    }
}