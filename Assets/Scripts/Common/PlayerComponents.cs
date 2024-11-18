using Unity.Entities;
using Unity.Entities.UniversalDelegates;
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
public struct JumpState : IComponentData
{
    public int State;
    [GhostField(Quantization = 0)] public float AirTime;
    public float AirTimeMax;
}