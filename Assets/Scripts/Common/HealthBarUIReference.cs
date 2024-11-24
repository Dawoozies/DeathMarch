using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public class HealthUIReference : ICleanupComponentData
{
    public GameObject Value;
}
public struct HealthUIOffset : IComponentData
{
    public float3 Value;
}