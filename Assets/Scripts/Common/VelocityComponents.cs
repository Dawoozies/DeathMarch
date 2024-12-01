using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct MoveVelocity : IComponentData
{
    [GhostField(Quantization = 0)] public float3 Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct JumpVelocity : IComponentData
{
    [GhostField(Quantization = 0)] public float3 Value;
}
public struct JumpStrength : IComponentData
{
    public float Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct Gravity : IComponentData
{
    public float3 Direction;
    public float Strength;
    public float StrengthMax;
    [GhostField(Quantization = 0)] public float AirTime;
    public float AirTimeMax;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct GravityVelocity : IComponentData
{
    [GhostField(Quantization = 0)] public float3 Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct GroundCheck : IComponentData
{
    [GhostField(Quantization = 0)] public float AirTime;
    public float2 GroundAngles;
}