using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Random = UnityEngine.Random;
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
                1 << 2  //enemy
        };
        RequireForUpdate<NetworkTime>();
    }
    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        //if (!EntityManager.WorldUnmanaged.IsCreated) return;

        foreach (var (aimInput, shootInput, cameraDirections, equippedWeaponData, weaponDataBuffer, weaponHitBuffer, entity) in SystemAPI.Query<
        PlayerAimInput,
        PlayerShootInput,
        PlayerCameraDirections,
        EquippedWeaponData,
        DynamicBuffer<WeaponDataBufferElement>,
        DynamicBuffer<WeaponHitResultBufferElement>
        >().WithAll<Simulate>().WithEntityAccess())
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

                        weaponHitBuffer.Add(new WeaponHitResultBufferElement
                        {
                            WeaponFiringPoint = firingPointWorldTransform.ValueRO.Position,
                            HitEntity = hit.Entity,
                            HitPosition = hit.Position,
                            HitNormal = hit.SurfaceNormal
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
                allHits.Dispose();

            }
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