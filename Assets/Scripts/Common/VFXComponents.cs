using Unity.Entities;
using Unity.Mathematics;

public struct BulletLineFXEvent : IComponentData
{
    public float3 WeaponFiringPoint;
    public float3 ShootDirection;
}