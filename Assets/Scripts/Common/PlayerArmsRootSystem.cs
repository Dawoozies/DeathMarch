using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
[BurstCompile]
public partial struct PlayerArmsRootSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //armature roots (right now) are child objects of the player entity
        foreach (var (parent, transform, followCamForward) in SystemAPI.Query<Parent, RefRW<LocalTransform>, RefRW<FollowCameraForward>>().WithAll<Simulate>())
        {
            var cameraDirections = SystemAPI.GetComponentRO<PlayerCameraDirections>(parent.Value);
            float dot = math.dot(transform.ValueRO.Forward(), cameraDirections.ValueRO.Forward);
            if(dot < 0)
                dot = 0;
            var cameraRot = quaternion.LookRotationSafe(cameraDirections.ValueRO.Forward, cameraDirections.ValueRO.Up);
            transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, cameraRot, dot);
            //transform.ValueRW.Rotation = quaternion.LookRotationSafe(cameraDirections.ValueRO.Forward, cameraDirections.ValueRO.Up);
        }
    }
}