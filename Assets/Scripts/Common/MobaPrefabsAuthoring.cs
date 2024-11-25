using Unity.Entities;
using UnityEngine;

public class MobaPrefabsAuthoring : MonoBehaviour
{
    public GameObject Champion;
    public GameObject HealthDisplayPrefab;
    public GameObject BulletLinePrefab;
    public class MobaPrefabsBaker : Baker<MobaPrefabsAuthoring>
    {
        public override void Bake(MobaPrefabsAuthoring authoring)
        {
            Entity prefabContainerEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(prefabContainerEntity, new MobaPrefabs
            {
                Champion = GetEntity(authoring.Champion, TransformUsageFlags.Dynamic)
            });
            AddComponentObject(prefabContainerEntity, new UIPrefabs
            {
                HealthDisplay = authoring.HealthDisplayPrefab 
            });
            AddComponentObject(prefabContainerEntity, new VFXPrefabs
            {
                BulletLine =  authoring.BulletLinePrefab
            });
        }
    }
}