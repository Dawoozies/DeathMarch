using UnityEngine;
using Unity.Entities;
public class GroundAuthoring : MonoBehaviour
{
    public class GroundBaker : Baker<GroundAuthoring>
    {
        public override void Bake(GroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<GroundTag>(entity);
        }
    }
}
public struct GroundTag : IComponentData {}