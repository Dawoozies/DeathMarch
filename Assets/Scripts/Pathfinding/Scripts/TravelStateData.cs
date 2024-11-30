// holds the travel state of an entity.
// it keeps track of whether or not the entity has reached its destination
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;

public struct TravelStateData : IComponentData
{
    public bool HasReachedEndOfPath;
}
