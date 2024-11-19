using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerLookSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, cameraDirections) in SystemAPI.Query<RefRW<LocalTransform>, PlayerCameraDirections>().WithAll<Simulate>())
        {
            //float3 lookPosFinal = lookPos.Value;
            //lookPosFinal.y = transform.ValueRO.Position.y;
            //float3 lookDir = math.normalize(lookPosFinal - transform.ValueRO.Position);
            float3 lookDir = float3.zero;
            lookDir.x = cameraDirections.Forward.x;
            lookDir.z = cameraDirections.Forward.y;
            transform.ValueRW.Rotation = quaternion.LookRotationSafe(lookDir, math.up());
        }
    }
}