// holds the index of the current waypoint in a path that an entity is moving towards
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;

public struct WaypointIndexData : IComponentData
{
    public int Index; // current path index (waypoint)
}
