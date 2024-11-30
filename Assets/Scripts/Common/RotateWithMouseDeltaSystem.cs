using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[BurstCompile]
public partial struct RotateWithMouseDeltaSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<OwnerChampTag>();
        state.RequireForUpdate<MainCameraTag>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity cameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
        Camera mainCamera = state.EntityManager.GetComponentObject<MainCamera>(cameraEntity).Value;
        foreach (var (_,originalRotation, rotationAxis,angles,localTransform) in SystemAPI.Query<
        RotateWithMouseDeltaTag, 
        OriginalRotation,
        RotationAxis,
        RefRW<MouseDeltaRotationAngles>, 
        RefRW<LocalTransform>
        >().WithAll<Simulate>())
        {
            localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(mainCamera.transform.forward*rotationAxis.Value.z, mainCamera.transform.up*rotationAxis.Value.y);
        }
    }
    public void OnDestroy(ref SystemState state)
    {
    }
}