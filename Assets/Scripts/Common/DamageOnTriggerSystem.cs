using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct DamageOnTriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EndSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var damageOnTriggerJob = new DamageOnTriggerJob
        {
            DamageOnTriggerLookup = SystemAPI.GetComponentLookup<DamageOnTrigger>(true),
            TeamLookup = SystemAPI.GetComponentLookup<MobaTeam>(true),
            AlreadyDamagedLookup = SystemAPI.GetBufferLookup<AlreadyDamagedEntity>(true),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageBufferElement>(true),
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
        };
        SimulationSingleton simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = damageOnTriggerJob.Schedule(simulationSingleton, state.Dependency);
    }
}
public struct DamageOnTriggerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<DamageOnTrigger> DamageOnTriggerLookup;
    [ReadOnly] public ComponentLookup<MobaTeam> TeamLookup;
    [ReadOnly] public BufferLookup<AlreadyDamagedEntity> AlreadyDamagedLookup;
    [ReadOnly] public BufferLookup<DamageBufferElement> DamageBufferLookup;
    public EntityCommandBuffer ECB;
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity damageDealingEntity;
        Entity damageReceivingEntity;
        // A is being damaged by B
        if(DamageBufferLookup.HasBuffer(triggerEvent.EntityA) && DamageOnTriggerLookup.HasComponent(triggerEvent.EntityB))
        {
            damageReceivingEntity = triggerEvent.EntityA;
            damageDealingEntity = triggerEvent.EntityB;
        }
        // B is being damaged by A
        else if(DamageBufferLookup.HasBuffer(triggerEvent.EntityB) && DamageOnTriggerLookup.HasComponent(triggerEvent.EntityA))
        {
            damageReceivingEntity = triggerEvent.EntityB;
            damageDealingEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        DynamicBuffer<AlreadyDamagedEntity> alreadyDamagedBuffer = AlreadyDamagedLookup[damageDealingEntity];
        foreach (AlreadyDamagedEntity alreadyDamagedEntity in alreadyDamagedBuffer)
        {
            if(alreadyDamagedEntity.Value.Equals(damageReceivingEntity)) return;
        }

        if(TeamLookup.TryGetComponent(damageDealingEntity, out var damageDealingTeam) && TeamLookup.TryGetComponent(damageReceivingEntity, out var damageReceivingTeam))
        {
            if(damageDealingTeam.Value == damageReceivingTeam.Value) return;
        }

        DamageOnTrigger damageOnTrigger = DamageOnTriggerLookup[damageDealingEntity];
        ECB.AppendToBuffer(damageReceivingEntity, new DamageBufferElement {Value = damageOnTrigger.Value});
        ECB.AppendToBuffer(damageDealingEntity, new AlreadyDamagedEntity {Value = damageReceivingEntity});
    }
}