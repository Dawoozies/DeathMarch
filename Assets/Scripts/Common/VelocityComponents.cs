using Unity.Entities;
using Unity.Mathematics;
public struct MoveVelocity : IComponentData
{
    public float3 Value;
}
public struct JumpVelocity : IComponentData
{
    public float3 Value;
}
public struct GravityVelocity : IComponentData
{
    public float3 Value;
}
public struct GravityDirection : IComponentData
{
    public float3 Value;
}
public struct GravityStrength : IComponentData
{
    public float Value;
}
public struct GroundCheck : IComponentData
{
    public bool isGrounded;
}