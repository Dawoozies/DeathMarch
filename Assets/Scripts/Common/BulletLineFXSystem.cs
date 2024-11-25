using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.VFX;
using Unity.NetCode;
using System.Linq;
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct BulletLineFXSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<VFXPrefabs>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (transform, _, entity) in SystemAPI.Query<
        LocalTransform,
        BulletLineFXTag
        >().WithNone<BulletLineReference>().WithEntityAccess())
        {
            //make vfx prefabs for players
            var bulletLinePrefab = SystemAPI.ManagedAPI.GetSingleton<VFXPrefabs>().BulletLine;
            var newBulletLineVFX = Object.Instantiate(bulletLinePrefab, transform.Position, Quaternion.identity);
            ecb.AddComponent(entity, new BulletLineReference { Value = newBulletLineVFX });
            ecb.AddComponent(entity, new WeaponShootListener { EventFired = false });
        }

        foreach (var (weaponShootListener, bulletLineRef, weaponHitResultBuffer) in SystemAPI.Query<
        WeaponShootListener,
        BulletLineReference,
        DynamicBuffer<WeaponHitResultBufferElement>
        >().WithAll<Simulate>())
        {
            if(weaponShootListener.EventFired || weaponHitResultBuffer.Length <= 0)
            {
                continue;
            }
            Debug.Log($"Firing bullet event IsServer={state.EntityManager.WorldUnmanaged.IsServer()}");
            Debug.Log($"Firing bullet event IsServer={state.EntityManager.WorldUnmanaged.IsClient()}");
            var bulletLineVfx = bulletLineRef.Value.GetComponent<VisualEffect>();
            VFXEventAttribute shootDirection = bulletLineVfx.CreateVFXEventAttribute();
            bulletLineVfx.SetVector3("StartPosition", weaponHitResultBuffer[weaponHitResultBuffer.Length - 1].HitPosition);
            shootDirection.SetVector3("ShootDirection", weaponHitResultBuffer[weaponHitResultBuffer.Length - 1].HitPosition - weaponHitResultBuffer[weaponHitResultBuffer.Length - 1].WeaponFiringPoint);
            bulletLineVfx.SendEvent("OnPlay", shootDirection);
        }
    }
}