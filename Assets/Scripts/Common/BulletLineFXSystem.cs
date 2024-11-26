using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.VFX;
using Unity.NetCode;
using Unity.Mathematics;
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct BulletLineFXSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginPresentationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<VFXPrefabs>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (transform, _, entity) in SystemAPI.Query<
        LocalTransform,
        BulletLineFXTag
        >().WithNone<BulletLineReference>().WithEntityAccess())
        {
            //make vfx prefabs for players
            var bulletLinePrefab = SystemAPI.ManagedAPI.GetSingleton<VFXPrefabs>().BulletLine;
            var newBulletLineVFX = Object.Instantiate(bulletLinePrefab, transform.Position + math.up()*(float)(SystemAPI.Time.ElapsedTime/100f), Quaternion.identity);
            ecb.AddComponent(entity, new BulletLineReference { Value = newBulletLineVFX });
            //This is the "subscription" though we have to do this if it is in the aspect
            ecb.AddComponent(entity, new BulletLine {hasFired = false});
            //ecb.AddComponent(entity, new WeaponShootListener { EventFired = false });
        }
        //we could see if we can check when someone ELSE 
        foreach (var (bulletLineRef, weaponHitResultBuffer, vfxEvent, entity) in SystemAPI.Query<
        BulletLineReference,
        DynamicBuffer<WeaponHitResultBufferElement>,
        RefRW<BulletLine>
        >().WithAll<Simulate>().WithEntityAccess())
        {
            if(vfxEvent.ValueRO.hasFired || weaponHitResultBuffer.Length <= 0)
            {
                continue;
            }
            ConsoleLog.Log($"vfxEvent firing!", state.EntityManager, entity);
            //Debug.Log($"Firing bullet event IsServer={state.EntityManager.WorldUnmanaged.IsServer()}");
            //Debug.Log($"Firing bullet event IsClient={state.EntityManager.WorldUnmanaged.IsClient()}");
            var bulletLineVfx = bulletLineRef.Value.GetComponent<VisualEffect>();
            VFXEventAttribute shootDirection = bulletLineVfx.CreateVFXEventAttribute();
            bulletLineVfx.SetVector3("StartPosition", weaponHitResultBuffer[weaponHitResultBuffer.Length - 1].WeaponFiringPoint);
            shootDirection.SetVector3("ShootDirection", weaponHitResultBuffer[weaponHitResultBuffer.Length - 1].HitPosition - weaponHitResultBuffer[weaponHitResultBuffer.Length - 1].WeaponFiringPoint);
            bulletLineVfx.SendEvent("OnPlay", shootDirection);

            vfxEvent.ValueRW.hasFired = true;
        }
    }
}