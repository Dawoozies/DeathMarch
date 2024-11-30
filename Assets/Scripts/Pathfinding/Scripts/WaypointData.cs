// holds a path of waypoints for an entity
//---------------------------------------------------------------------------------------------//

using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(16)]
public struct WaypointData : IBufferElementData
{
    public float3 Point;
}