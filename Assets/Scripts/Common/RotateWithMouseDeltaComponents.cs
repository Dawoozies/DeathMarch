using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
public struct RotateWithMouseDeltaTag : IComponentData {}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct OriginalRotation : IComponentData
{
    [GhostField(Quantization = 0)] public float4 Value;
}
public struct RotationAxis : IComponentData
{
    public float3 Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct MouseDeltaRotationAngles : IComponentData
{
    [GhostField(Quantization = 0)] public float2 Value;
}