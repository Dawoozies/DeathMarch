using Unity.Entities;
public struct HordeMove : IComponentData
{
    public float MoveSpeed;
    public float ClimbSpeed;
    public float Gravity;
}