using Unity.Entities;
using UnityEngine;
public class BulletLineReference : ICleanupComponentData
{
    public GameObject Value;
}
public class WeaponShootListener : IComponentData
{
    public bool EventFired;
}