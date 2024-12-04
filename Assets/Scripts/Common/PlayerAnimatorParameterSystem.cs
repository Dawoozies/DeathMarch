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
        foreach (var (parent, allParams, entity) in SystemAPI.Query<RefRO<Parent>, DynamicBuffer<AnimatorControllerParameterComponent>>().WithAll<Simulate>().WithEntityAccess())
        {
            Entity armsRootParent = SystemAPI.GetComponent<Parent>(parent.ValueRO.Value).Value;
            RefRO<EquippedWeaponData> equippedWeaponData = SystemAPI.GetComponentRO<EquippedWeaponData>(armsRootParent);
            DynamicBuffer<WeaponDataBufferElement> weaponDataBuffer = SystemAPI.GetBuffer<WeaponDataBufferElement>(armsRootParent);
            WeaponDataBufferElement weaponDataBufferElement = weaponDataBuffer[equippedWeaponData.ValueRO.EquippedWeaponIndex];
            // 0 == Aiming (bool)
            RefRO<PlayerAimInput> aimInput = SystemAPI.GetComponentRO<PlayerAimInput>(armsRootParent);
            var aimingParameter = allParams[0];
            aimingParameter.FloatValue = math.clamp(aimInput.ValueRO.HeldTime/weaponDataBufferElement.AimHeldTimeMax, 0f, 1f);
            allParams.ElementAt(0) = aimingParameter;

            // 1 == ShootHeldTime (float)
            RefRO<PlayerShootInput> shootInput = SystemAPI.GetComponentRO<PlayerShootInput>(armsRootParent);
            var shootHeldTimeParameter = allParams[1];
            shootHeldTimeParameter.FloatValue = math.clamp(shootInput.ValueRO.HeldTime / weaponDataBufferElement.ShootHeldTimeMax, 0f, 1f);
            allParams.ElementAt(1) = shootHeldTimeParameter;

            Debug.Log($"AimHeldTime={aimingParameter.FloatValue}");
            ////Debug.Log($"Parent={parent.ValueRO.Value.Index}:{parent.ValueRO.Value.Version}Entity={entity.Index}:{entity.Version}aim={aimInput.ValueRO.Value}");
        }
    }
}