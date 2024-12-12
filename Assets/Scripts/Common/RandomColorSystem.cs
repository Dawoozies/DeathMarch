using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial struct RandomColorSystem : ISystem
{
    private Random random;
    public void OnCreate(ref SystemState state)
    {
        random = Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime);
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_,entity) in SystemAPI.Query<RandomColorTag>().WithAll<Simulate>().WithEntityAccess())
        {
            float r = random.NextFloat();
            float g = random.NextFloat();
            float b = random.NextFloat();
            ecb.AddComponent(entity, new URPMaterialPropertyBaseColor {Value = new float4(r, g, b, 1)});
            ecb.RemoveComponent<RandomColorTag>(entity);
        }
        ecb.Playback(state.EntityManager);
    }
    public void OnDestroy(ref SystemState state)
    {
    }
}