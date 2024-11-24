using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Burst;
using Unity.NetCode;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct GroundCheckSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (groundCheck, entity) in SystemAPI.Query<
        RefRW<GroundCheck>
        >().WithAll<Simulate>().WithEntityAccess())
        {
            NativeReference<int> numCollisionEvents = new NativeReference<int>(0, Allocator.TempJob);
            var job = new GroundCollisionEvents
            {
                GroundTagLookup = SystemAPI.GetComponentLookup<GroundTag>(true),
                NumCollisionEvents = numCollisionEvents,
                CheckingEntity = entity
            };
            job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency).Complete();
            if (numCollisionEvents.Value > 0)
            {
                groundCheck.ValueRW.AirTime = 0;
            }
            else
            {
                groundCheck.ValueRW.AirTime += SystemAPI.Time.DeltaTime;
            }

            numCollisionEvents.Dispose();
        }
    }
}
[BurstCompile]
public partial struct GroundCollisionEvents : ICollisionEventsJob
{
    [ReadOnly] public ComponentLookup<GroundTag> GroundTagLookup;
    public NativeReference<int> NumCollisionEvents;
    [ReadOnly] public Entity CheckingEntity;
    public void Execute(CollisionEvent collisionEvent)
    {
        if (collisionEvent.EntityA == CheckingEntity && GroundTagLookup.HasComponent(collisionEvent.EntityB))
        {
            NumCollisionEvents.Value++;
        }
    }
}