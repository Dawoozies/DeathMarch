using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
public struct EquippedWeaponData : IComponentData
{
    public int EquippedWeaponIndex;
}
public struct WeaponDataBufferElement : IBufferElementData
{
    public Entity WeaponFiringPoint;
    public float ShootHeldTimeMax;
    public float2 HorizontalBounds;
    public float2 VerticalBounds;
    public int Ammo;
    public float RateOfFire;
    public float Range;
    public int Penetration;
}
public struct WeaponHitResultBufferElement : IBufferElementData
{
    public float3 WeaponFiringPoint;
    public Entity HitEntity;
    public float3 HitPosition;
    public float3 HitNormal;
}
public struct WeaponHitResultTime : IComponentData
{
    public float ElapsedTime;
}