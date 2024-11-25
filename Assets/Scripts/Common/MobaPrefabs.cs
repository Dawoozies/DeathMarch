using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;
public struct MobaPrefabs : IComponentData
{
    public Entity Champion;
}
public class UIPrefabs : IComponentData
{
    public GameObject HealthDisplay;
}
public class VFXPrefabs : IComponentData
{
    public GameObject BulletLine;
}