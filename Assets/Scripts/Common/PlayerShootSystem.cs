using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.VFX;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class PlayerShootSystem : SystemBase
{
    private CollisionFilter BulletFilter;
    protected override void OnCreate()
    {
        BulletFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5, //raycasts
            CollidesWith =
                1 << 0 | //ground plane
                1 << 2  //enemy
        };
        RequireForUpdate<NetworkTime>();
    }
    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        //if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        //if (!EntityManager.WorldUnmanaged.IsCreated) return;

        foreach (var (aimInput, shootInput, cameraDirections, equippedWeaponData, weaponDataBuffer, weaponHitBuffer) in SystemAPI.Query<
        PlayerAimInput,
        PlayerShootInput,
        PlayerCameraDirections,
        EquippedWeaponData,
        DynamicBuffer<WeaponDataBufferElement>,
        DynamicBuffer<WeaponHitResultBufferElement>
        >().WithAll<Simulate>())
        {
            //if(!EntityManager.WorldUnmanaged.IsServer()) continue;
            //get the firing point LocalToWorld

            if (shootInput.Shoot.IsSet)
            {
                WeaponDataBufferElement weaponDataBufferElement = weaponDataBuffer[equippedWeaponData.EquippedWeaponIndex];
                Entity weaponFiringPoint = weaponDataBufferElement.WeaponFiringPoint;
                RefRO<LocalToWorld> firingPointWorldTransform = SystemAPI.GetComponentRO<LocalToWorld>(weaponFiringPoint);
                // Shot direction calculation
                float3 shotDir = cameraDirections.Forward;
                float2 sway = shootInput.ShootSway;
                if(aimInput.Value)
                {
                    sway /= 1f+weaponDataBufferElement.AimDownSightStability;
                }
                shotDir += cameraDirections.Right *sway.x;
                shotDir += cameraDirections.Up * sway.y;
                float3 shotVector = shotDir * weaponDataBufferElement.Range;
                //We are getting input client and server side. But it's important we only write/change
                //things server side. Any change of data must only be done on the server.
                //BUT any effects must be done client + server side!
                //The only thing we must limit to the server is the reduction of hitpoints on hit entities
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    //RAYCAST PASS
                    CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
                    RaycastInput bulletCast = new RaycastInput
                    {
                        Start = firingPointWorldTransform.ValueRO.Position,
                        End = firingPointWorldTransform.ValueRO.Position + shotVector,
                        Filter = BulletFilter
                    };
                    NativeList<RaycastHit> allHits = new NativeList<RaycastHit>(Allocator.Temp);
                    weaponHitBuffer.Clear();
                    if (collisionWorld.CastRay(bulletCast, ref allHits))
                    {
                        allHits.Sort(new RaycastHitComparer
                        {
                            RaycastStart = firingPointWorldTransform.ValueRO.Position
                        });
                        //try sorting with fraction after this so we dont need to do distance calculations again
                        int penetrationsLeft = weaponDataBufferElement.Penetration;
                        foreach (var hit in allHits)
                        {
                            //float d = math.distance(firingPointWorldTransform.ValueRO.Position, hit.Position);
                            //if (EntityManager.WorldUnmanaged.IsServer())
                            //{
                            //    UnityEngine.Debug.Log($"d={d}m Hit.Position={hit.Position} Hit.Entity={hit.Entity.Index}:{hit.Entity.Version} IsServer={EntityManager.WorldUnmanaged.IsServer()}");
                            //}
                            //if (EntityManager.WorldUnmanaged.IsClient())
                            //{
                            //    UnityEngine.Debug.LogError($"d={d}m Hit.Position={hit.Position} Hit.Entity={hit.Entity.Index}:{hit.Entity.Version} IsServer={EntityManager.WorldUnmanaged.IsServer()}");
                            //}
                            //UnityEngine.Debug.LogError($"Hit.Position={hit.Position}");
                            //Only do these on the server!!!!
                            if (EntityManager.WorldUnmanaged.IsServer())
                            {
                                if (SystemAPI.HasComponent<CurrentHitPoints>(hit.Entity))
                                {
                                    var hp = SystemAPI.GetComponentRW<CurrentHitPoints>(hit.Entity);
                                    hp.ValueRW.Value--;
                                }
                            }
                            HitType hitType = HitType.Miss;
                            if (SystemAPI.HasComponent<CurrentHitPoints>(hit.Entity))
                            {
                                hitType = HitType.Normal;
                            }
                            weaponHitBuffer.Add(new WeaponHitResultBufferElement
                            {
                                WeaponFiringPoint = firingPointWorldTransform.ValueRO.Position,
                                HitPosition = hit.Position,
                                HitNormal = hit.SurfaceNormal,
                                HitType = hitType
                            });

                            penetrationsLeft--;
                            if (penetrationsLeft <= 0)
                                break;
                        }
                    }
                    else
                    {
                        weaponHitBuffer.Add(new WeaponHitResultBufferElement
                        {
                            WeaponFiringPoint = firingPointWorldTransform.ValueRO.Position,
                            HitPosition = firingPointWorldTransform.ValueRO.Position + shotVector,
                        });
                    }
                    allHits.Dispose();
                }
                //remember that its only the other clients that cant see the host
                if (networkTime.IsFirstPredictionTick)
                {
                    //what does this need
                    //it just needs the weapon hit buffer?
                    VisualEffect vfx = EffectsManager.ins.GetEffect(0);
                    VFXEventAttribute shootDirectionAttribute = vfx.CreateVFXEventAttribute();
                    if (weaponHitBuffer.Length > 0)
                    {
                        shootDirectionAttribute.SetVector3("ShootDirection", weaponHitBuffer[weaponHitBuffer.Length - 1].HitPosition - weaponHitBuffer[weaponHitBuffer.Length - 1].WeaponFiringPoint);
                    }
                    else
                    {
                        shootDirectionAttribute.SetVector3("ShootDirection", shotVector);
                    }
                    shootDirectionAttribute.SetVector3("StartPosition", firingPointWorldTransform.ValueRO.Position);
                    vfx.SendEvent("OnPlay", shootDirectionAttribute);

                    if (weaponHitBuffer.Length > 0)
                    {
                        VisualEffect hitMarkerVfx = EffectsManager.ins.GetEffect(1);
                        foreach (var item in weaponHitBuffer)
                        {
                            VFXEventAttribute eventAttribute = hitMarkerVfx.CreateVFXEventAttribute();
                            eventAttribute.SetVector3("StartPosition", item.HitPosition);
                            eventAttribute.SetBool("HitSuccess", item.HitType == HitType.Normal);
                            hitMarkerVfx.SendEvent("OnPlay", eventAttribute);
                        }
                    }
                }
                //float3 shootDirection = shotVector;
                //if (allHits.Length > 0)
                //{
                //    shootDirection = allHits[allHits.Length - 1].Position - firingPointWorldTransform.ValueRO.Position;
                //}
                //foreach (var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>().WithAll<BulletLineFX>())
                //{
                //    //UnityEngine.Debug.LogError($"VFX RUN isServer:{EntityManager.WorldUnmanaged.IsServer()} isClient:{EntityManager.WorldUnmanaged.IsClient()}");
                //    //UnityEngine.Debug.LogError($"MANAGED VFX RUN isServer:{EntityManager.World.IsServer()} isClient:{EntityManager.World.IsClient()}");
                //    vfx.Value.SetVector3("StartPosition", firingPointWorldTransform.ValueRO.Position);
                //    VFXEventAttribute shootDirectionAttribute = vfx.Value.CreateVFXEventAttribute();
                //    float3 shootDirVector = shotVector;
                //    if (allHits.Length > 0)
                //    {
                //        shootDirVector = allHits[allHits.Length - 1].Position - firingPointWorldTransform.ValueRO.Position;
                //    }
                //    shootDirectionAttribute.SetVector3(ShootDirAttribute, shootDirVector);
                //    vfx.Value.SendEvent("OnPlay", shootDirectionAttribute);
                //}
            }
            //lets see if we can get it to work here :)
        }
    }
}
public struct RaycastHitComparer : IComparer<RaycastHit>
{
    public float3 RaycastStart;
    public int Compare(RaycastHit a, RaycastHit b)
    {
        return a.Fraction.CompareTo(b.Fraction);
    }
}