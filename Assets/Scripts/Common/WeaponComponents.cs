using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
public struct EquippedWeaponData : IComponentData
{
    public int EquippedWeaponIndex;
}
public struct WeaponDataBufferElement : IBufferElementData
{
    public Entity WeaponFiringPoint;
    public Entity WeaponAimDownSightPosition;
    public float ShootHeldTimeMax;
    public float2 HorizontalBounds;
    public float2 VerticalBounds;
    public int Ammo;
    public float RateOfFire;
    public float Range;
    public int Penetration;
    public float AimDownSightStability;
}
public struct WeaponHitResultBufferElement : IBufferElementData
{
    public float3 WeaponFiringPoint;
    //public Entity HitEntity;
    public float3 HitPosition;
    public float3 HitNormal;
    public HitType HitType;
}
public struct WeaponEventBufferElement : IBufferElementData
{
    public bool hasFired;
}
public enum HitType
{
    Miss = 0,
    Normal = 1,
    Critical = 2,
}