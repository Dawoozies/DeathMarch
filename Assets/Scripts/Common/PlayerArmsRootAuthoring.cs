using UnityEngine;
using Unity.Entities;
public class PlayerArmsRootAuthoring : MonoBehaviour
{
    public class ComponentBaker : Baker<PlayerArmsRootAuthoring>
    {
        public override void Bake(PlayerArmsRootAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FollowCameraForward>(entity);
        }
    }
}
public struct FollowCameraForward : IComponentData
{
}
//slerp towards camera forwards, give smoothing values etc