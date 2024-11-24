using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
public class BulletLineFXAuthoring : MonoBehaviour
{
    public class ComponentBaker : Baker<BulletLineFXAuthoring>
    {
        public override void Bake(BulletLineFXAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BulletLineFX{VisualEffectEntity = entity});
        }
    }
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.All)]
public struct BulletLineFX : IComponentData 
{
    public Entity VisualEffectEntity;
}