using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerLookInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public float3 Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerMoveInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public float2 Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct MoveSpeed : IComponentData
{
    [GhostField(Quantization = 0)] public float Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerJumpInput : IInputComponentData 
{ 
    [GhostField] public bool Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerAimInput : IInputComponentData
{
    [GhostField] public bool Value;
    [GhostField(Quantization = 0)] public float HeldTime;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct JumpState : IComponentData
{
    [GhostField] public int State;
    [GhostField(Quantization = 0)] public float AirTime;
    public float AirTimeMax;
    public float CoyoteTime;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerCameraDirections : IInputComponentData
{
    [GhostField(Quantization = 0)] public float3 Forward;
    [GhostField(Quantization = 0)] public float3 Right;
    [GhostField(Quantization = 0)] public float3 Up;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerShootInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public float HeldTime;
    [GhostField(Quantization = 0)] public float ShootTime;
    [GhostField] public InputEvent Shoot;
    [GhostField(Quantization = 0)] public float2 ShootSway;
}