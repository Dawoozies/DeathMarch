using UnityEngine;
using Unity.Entities;
public class DamageOnCollisionAuthoring : MonoBehaviour
{
    public class ComponentBaker : Baker<DamageOnCollisionAuthoring>
    {
        public override void Bake(DamageOnCollisionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}
public struct DamageOnCollision : IComponentData
{
    public int Value;
}