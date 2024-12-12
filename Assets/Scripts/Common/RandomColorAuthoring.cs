using Unity.Entities;
using UnityEngine;

public class RandomColorAuthoring : MonoBehaviour
{
    private class RandomColorAuthoringBaker : Baker<RandomColorAuthoring>
    {
        public override void Bake(RandomColorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent<RandomColorTag>(entity);
        }
    }
}
public struct RandomColorTag : IComponentData {}