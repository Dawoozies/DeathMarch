//using Unity.Entities;
//using UnityEngine;

//public struct FollowerSpawnerOptsData : IComponentData
//{
//    public Entity PrefabEntity;
//}


//public class FollowerGridSpawnerAuthoring : MonoBehaviour
//{
//    public GameObject Prefab;
//}


//public class FollowerGridSpawnerBaker : Baker<FollowerGridSpawnerAuthoring>
//{
//    public override void Bake(FollowerGridSpawnerAuthoring authoring)
//    {
//        Entity prefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
//        Entity spawnerEntity = GetEntity(TransformUsageFlags.Dynamic);

//        AddComponent(spawnerEntity, new FollowerSpawnerOptsData { PrefabEntity = prefabEntity });
//    }
//}