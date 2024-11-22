using Unity.Entities;
using Unity.NetCode;
using Rukhanka;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerAnimatorParameterSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //keeping this comment here to remember in case i ever fuck this up again lmao
        //this code does(did) not run server side
        //because the tag OwnedAnimatorTag is only run on the client
        //This is because we attach OwnedAnimatorTag based on OwnerChampTag
        //However we should not do this
        foreach(var (parent, allParams, entity) in SystemAPI.Query<RefRO<Parent>, DynamicBuffer<AnimatorControllerParameterComponent>>().WithAll<Simulate>().WithEntityAccess())
        {
            // 0 == Aiming (bool)
            RefRO<PlayerAimInput> aimInput = SystemAPI.GetComponentRO<PlayerAimInput>(parent.ValueRO.Value);
            var aimingParameter = allParams[0];
            aimingParameter.BoolValue = aimInput.ValueRO.Value;
            allParams.ElementAt(0) = aimingParameter;

            // 1 == ShootHeldTime (float)
            RefRO<PlayerShootInput> shootInput = SystemAPI.GetComponentRO<PlayerShootInput>(parent.ValueRO.Value);
            RefRO<EquippedWeaponData> equippedWeaponData = SystemAPI.GetComponentRO<EquippedWeaponData>(parent.ValueRO.Value);
            var shootHeldTimeParameter = allParams[1];
            shootHeldTimeParameter.FloatValue = math.clamp(shootInput.ValueRO.HeldTime/equippedWeaponData.ValueRO.ShootHeldTimeMax, 0f, 1f);
            allParams.ElementAt(1) = shootHeldTimeParameter;
            
            ////Debug.Log($"Parent={parent.ValueRO.Value.Index}:{parent.ValueRO.Value.Version}Entity={entity.Index}:{entity.Version}aim={aimInput.ValueRO.Value}");
        }
    } 
}
public partial struct AnimatorParamDebug : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //this code does not run server side
        //foreach(var (localToWorld, allParams,entity) in SystemAPI.Query<LocalToWorld, DynamicBuffer<AnimatorControllerParameterComponent>>().WithAll<Simulate>().WithEntityAccess())
        //{
        //    string output = $"E{entity.Index}:{entity.Version}P={Unity.Mathematics.math.round(localToWorld.Position)}W={state.World.Name}";
        //    foreach(AnimatorControllerParameterComponent item in allParams)
        //    {
        //        output += $"\n Param:Hash{item.hash}:F={item.FloatValue}:B={item.BoolValue}:I={item.IntValue}";
        //    }
        //    Debug.LogError(output);
        //}
    }     
}