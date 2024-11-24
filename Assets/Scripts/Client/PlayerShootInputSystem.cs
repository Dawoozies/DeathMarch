using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
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
        RefRO<PlayerShootInput> currentShootInput = SystemAPI.GetComponentRO<PlayerShootInput>(playerEntity);
        float currentHeldTime = currentShootInput.ValueRO.HeldTime;
        float currentShootTime = currentShootInput.ValueRO.ShootTime;
        bool aimInput = SystemAPI.GetComponentRO<PlayerAimInput>(playerEntity).ValueRO.Value;
        DynamicBuffer<WeaponDataBufferElement> weaponBuffer = SystemAPI.GetBuffer<WeaponDataBufferElement>(playerEntity);
        RefRO<EquippedWeaponData> equippedWeaponData = SystemAPI.GetComponentRO<EquippedWeaponData>(playerEntity);
        WeaponDataBufferElement weaponDataBufferElement = weaponBuffer[equippedWeaponData.ValueRO.EquippedWeaponIndex];

        float nextHeldTime = _inputActions.Player.Shoot.IsPressed() && aimInput ? currentHeldTime + SystemAPI.Time.DeltaTime : 0;
        PlayerShootInput nextShootInput = new PlayerShootInput
        {
            HeldTime = nextHeldTime
        };
        float nextShootTime = _inputActions.Player.Shoot.IsPressed() && aimInput ? currentShootTime + SystemAPI.Time.DeltaTime : 0;
        if (currentHeldTime == 0f && currentShootTime == 0f && _inputActions.Player.Shoot.WasPressedThisFrame())
        {
            nextShootInput.Shoot.Set();
            //Debug.LogError($"Shot happening 1st if statement at time {SystemAPI.Time.ElapsedTime}");
        }
        if (currentShootTime >= 1f / weaponDataBufferElement.RateOfFire)
        {
            nextShootInput.Shoot.Set();
            nextShootTime = 0f;
            //Debug.LogError($"Shot happening 2nd if statement {SystemAPI.Time.ElapsedTime}");
        }
        nextShootInput.ShootTime = nextShootTime;

        float shootHeldTimeFactor = nextHeldTime / weaponDataBufferElement.ShootHeldTimeMax;
        float xSwayBound = math.lerp(weaponDataBufferElement.HorizontalBounds.x, weaponDataBufferElement.HorizontalBounds.y, math.clamp(shootHeldTimeFactor, 0f, 1f));
        float ySwayBound = math.lerp(weaponDataBufferElement.VerticalBounds.x, weaponDataBufferElement.VerticalBounds.y, math.clamp(shootHeldTimeFactor, 0f, 1f));
        float2 shootSway = new float2
        (
            UnityEngine.Random.Range(-xSwayBound,xSwayBound),
            UnityEngine.Random.Range(-ySwayBound,ySwayBound)
        );
        nextShootInput.ShootSway = shootSway;
        //Doing the sway calculation here lets us prevent client deviating from server

        EntityManager.SetComponentData(playerEntity, nextShootInput);
        //Debug.Log($"CurrentHeldTime = {currentHeldTime}");
    }
}
