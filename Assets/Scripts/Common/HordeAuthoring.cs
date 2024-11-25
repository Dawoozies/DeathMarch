using UnityEngine;
using Unity.Entities;
public class HordeAuthoring : MonoBehaviour
{
    public class ComponentBaker : Baker<HordeAuthoring>
    {
        public override void Bake(HordeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}