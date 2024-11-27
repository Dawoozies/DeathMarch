using UnityEngine;
using Unity.Entities;
public class HordeAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float climbSpeed;
    public float gravity;
    public class ComponentBaker : Baker<HordeAuthoring>
    {
        public override void Bake(HordeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HordeMove 
            {
                MoveSpeed = authoring.moveSpeed,
                ClimbSpeed = authoring.climbSpeed,
                Gravity = authoring.gravity
            });
            AddComponent(entity, new GroundCheck
            {
                AirTime = 0
            });
        }
    }
}