// MoveSpeedData holds the movement speed component of an object
// MoveSpeedModifierData can be added to an entity to change its move-speed
//---------------------------------------------------------------------------------------------//

using Unity.Entities;

public struct MoveSpeedData : IComponentData
{
    public float Speed;
}

public struct MaxMoveSpeedData : IComponentData
{
    public float Speed;
}

public struct MoveSpeedModifierData : IBufferElementData
{
    public bool remove; // removes the modifier (changes the speed by one increment)
    public float amount; // amount to change the speed.  0.2 increases the speed by 20%.  -0.2 decreases it by 20%.
}
