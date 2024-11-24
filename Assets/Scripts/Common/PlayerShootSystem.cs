using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using Random = UnityEngine.Random;
using RaycastHit = Unity.Physics.RaycastHit;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class BulletLineFXSystem : SystemBase
{
    private readonly ExposedProperty ShootDirAttribute = "ShootDirection";
    private CollisionFilter BulletFilter;
    protected override void OnCreate()
    {
        RequireForUpdate<BulletLineFX>();
        BulletFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5, //raycasts
            CollidesWith =
                1 << 0 | //ground plane
                1 << 2 | //enemy
                1 << 4 //structure
        };
        RequireForUpdate<NetworkTime>();
    }
    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        if (!EntityManager.WorldUnmanaged.IsCreated) return;


        foreach (var (aimInput, shootInput, cameraDirections, equippedWeaponData, weaponDataBuffer) in SystemAPI.Query<
        PlayerAimInput,
        PlayerShootInput,
        PlayerCameraDirections,
        EquippedWeaponData,
        DynamicBuffer<WeaponDataBufferElement>
        >().WithAll<Simulate>())
        {
            //if(!EntityManager.WorldUnmanaged.IsServer()) continue;

            if (aimInput.Value && shootInput.Shoot.IsSet)
            {
                //We are getting input client and server side. But it's important we only write/change
                //things server side. Any change of data must only be done on the server.
                //BUT any effects must be done client + server side!
                //The only thing we must limit to the server is the reduction of hitpoints on hit entities

                //get the firing point LocalToWorld
                WeaponDataBufferElement weaponDataBufferElement = weaponDataBuffer[equippedWeaponData.EquippedWeaponIndex];
                Entity weaponFiringPoint = weaponDataBufferElement.WeaponFiringPoint;
                RefRO<LocalToWorld> firingPointWorldTransform = SystemAPI.GetComponentRO<LocalToWorld>(weaponFiringPoint);
                // Shot direction calculation
                float3 shotDir = new float3(cameraDirections.Forward.x, 0f, cameraDirections.Forward.y);
                shotDir += new float3(cameraDirections.Right.x, 0f, cameraDirections.Right.y) * shootInput.ShootSway.x;
                shotDir += math.up() * shootInput.ShootSway.y;

                float3 shotVector = shotDir * weaponDataBufferElement.Range;

                //RAYCAST PASS
                CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
                RaycastInput bulletCast = new RaycastInput
                {
                    Start = firingPointWorldTransform.ValueRO.Position,
                    End = firingPointWorldTransform.ValueRO.Position + shotVector,
                    Filter = BulletFilter
                };
                NativeList<RaycastHit> allHits = new NativeList<RaycastHit>(Allocator.Temp);
                //float3 closestHit = firingPointWorldTransform.ValueRO.Position + shotVector; // must be the furthest point to start
                //float3 furthestHit = firingPointWorldTransform.ValueRO.Position; //must be the closest point to start
                if (collisionWorld.CastRay(bulletCast, ref allHits))
                {
                    int penetrationsLeft = weaponDataBufferElement.Penetration;
                    foreach (var hit in allHits)
                    {
                        UnityEngine.Debug.LogError($"Hit.Position={hit.Position}");
                        //Only do these on the server!!!!
                        if (EntityManager.WorldUnmanaged.IsServer())
                        {
                            if (SystemAPI.HasComponent<CurrentHitPoints>(hit.Entity))
                            {
                                var hp = SystemAPI.GetComponentRW<CurrentHitPoints>(hit.Entity);
                                hp.ValueRW.Value--;
                            }
                        }

                        penetrationsLeft--;
                        if (penetrationsLeft <= 0)
                            break;
                    }
                }
                // VFX PASS
                int vfxCount = 0;
                foreach(var visualEffectComponent in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>())
                {
                    vfxCount++;
                }
                
                UnityEngine.Debug.LogError($"Before VFX isServer:{EntityManager.WorldUnmanaged.IsServer()} isClient:{EntityManager.WorldUnmanaged.IsClient()} vfxCount:{vfxCount}");
                UnityEngine.Debug.LogError($"Before Managed VFX isServer:{EntityManager.World.IsServer()} isClient:{EntityManager.World.IsClient()} vfxCount:{vfxCount}");
                foreach (var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>().WithAll<BulletLineFX>())
                {
                    //UnityEngine.Debug.LogError($"VFX RUN isServer:{EntityManager.WorldUnmanaged.IsServer()} isClient:{EntityManager.WorldUnmanaged.IsClient()}");
                    //UnityEngine.Debug.LogError($"MANAGED VFX RUN isServer:{EntityManager.World.IsServer()} isClient:{EntityManager.World.IsClient()}");
                    vfx.Value.SetVector3("StartPosition", firingPointWorldTransform.ValueRO.Position);
                    VFXEventAttribute shootDirectionAttribute = vfx.Value.CreateVFXEventAttribute();
                    float3 shootDirVector = shotVector;
                    if (allHits.Length > 0)
                    {
                        shootDirVector = allHits[allHits.Length - 1].Position - firingPointWorldTransform.ValueRO.Position;
                    }
                    shootDirectionAttribute.SetVector3(ShootDirAttribute, shootDirVector);
                    vfx.Value.SendEvent("OnPlay", shootDirectionAttribute);
                }
                allHits.Dispose();
            }
        }
    }
}