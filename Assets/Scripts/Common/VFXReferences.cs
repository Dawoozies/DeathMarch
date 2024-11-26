using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
public class BulletLineReference : ICleanupComponentData
{
    public GameObject Value;
}
public struct BulletLine : IComponentData
{
    public bool hasFired;
}
public readonly partial struct VFXEventAspect : IAspect
{
    readonly RefRW<BulletLine> BulletLineEvent;
    public bool BulletLineEventFired
    {
        get => BulletLineEvent.ValueRO.hasFired;
        set => BulletLineEvent.ValueRW.hasFired = value;
    }
    public bool EventsFired
    {
        set {
            BulletLineEventFired = value;
        }
    }
}