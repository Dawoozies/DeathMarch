using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
public class ChampAuthoring : MonoBehaviour
{
    [Header("Player Data")]
    public float moveSpeed;
    public float jumpAirTimeMax;
    public float jumpStrength;
    public float coyoteTime;
    public Vector3 gravityDirection;
    public float gravityStrength;
    public Vector2 groundAngles;
    public float gravityStrengthMax;
    public float gravityAirTimeMax;

    [Header("Equipped Weapon Data")]
    public int equippedWeaponIndex;
    public WeaponData[] allWeaponData;
    [System.Serializable]
    public class WeaponData
    {
        public GameObject firingPoint;
        public GameObject aimDownSightPosition;
        public float shootHeldTimeMax;
        public float aimHeldTimeMax;
        [Tooltip("Min and max horizontal accuracy bounds")]
        public Vector2 horizontalBounds;
        [Tooltip("Min and max vertical accuracy bounds")]
        public Vector2 verticalBounds;
        public int ammo;
        [Tooltip("Measured in rounds per second")]
        public float rateOfFire;
        public float range;
        public int penetration;
        [Tooltip("Shoot sway gets divided by this number on aim down sight")]
        public float aimDownSightStability;
    }
    public class ChampBaker : Baker<ChampAuthoring>
    {
        public override void Bake(ChampAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ChampTag>(entity);
            AddComponent<NewChampTag>(entity);
            AddComponent<MobaTeam>(entity);
            AddComponent<URPMaterialPropertyBaseColor>(entity);
            //AddComponent<AbilityInput>(entity);

            //Look to mouse components
            AddComponent<PlayerLookInput>(entity);
            AddComponent<PlayerCameraDirections>(entity);

            //Planar Movement components
            AddComponent<PlayerMoveInput>(entity);
            AddComponent(entity, new MoveSpeed {Value = authoring.moveSpeed});
            AddComponent(entity, new MoveVelocity {Value = float3.zero});

            //Jump components
            AddComponent<PlayerJumpInput>(entity);
            AddComponent(entity, new JumpState {
                State = 0,
                AirTime = 0,
                AirTimeMax = authoring.jumpAirTimeMax,
                CoyoteTime = authoring.coyoteTime
            });
            AddComponent(entity, new JumpVelocity {Value = float3.zero});
            AddComponent(entity, new JumpStrength {Value = authoring.jumpStrength});

            //Ground check components
            AddComponent(entity, new GroundCheck
            {
                AirTime = 0,
                GroundAngles = authoring.groundAngles
            });

            //Gravity components
            AddComponent(entity, new GravityVelocity {Value = float3.zero});
            AddComponent(entity, new Gravity
            {
                Direction = authoring.gravityDirection,
                Strength = authoring.gravityStrength,
                StrengthMax = authoring.gravityStrengthMax,
                AirTime = 0,
                AirTimeMax = authoring.gravityAirTimeMax
            });

            //Player Aim Input
            AddComponent(entity, new PlayerAimInput{
                Value = false,
                HeldTime = 0f
            });

            //Player Shoot Input
            AddComponent(entity, new PlayerShootInput
            {
                HeldTime = 0f,
                ShootTime = 0f
            });

            //Equipped Weapon Data
            DynamicBuffer<WeaponDataBufferElement> weaponDataBuffer = AddBuffer<WeaponDataBufferElement>(entity);
            for(int i = 0; i < authoring.allWeaponData.Length; i++)
            {
                WeaponData weaponData = authoring.allWeaponData[i];
                Entity weaponFiringPointEntity = GetEntity(weaponData.firingPoint, TransformUsageFlags.Dynamic);
                Entity weaponAimDownSightPosition = GetEntity(weaponData.aimDownSightPosition, TransformUsageFlags.Dynamic);
                WeaponDataBufferElement weaponDataBufferElement = new WeaponDataBufferElement 
                {
                    WeaponFiringPoint = weaponFiringPointEntity,
                    WeaponAimDownSightPosition = weaponAimDownSightPosition,
                    ShootHeldTimeMax = weaponData.shootHeldTimeMax,
                    AimHeldTimeMax = weaponData.aimHeldTimeMax,
                    HorizontalBounds = weaponData.horizontalBounds,
                    VerticalBounds = weaponData.verticalBounds,
                    Ammo = weaponData.ammo,
                    RateOfFire = weaponData.rateOfFire,
                    Range = weaponData.range,
                    Penetration = weaponData.penetration,
                    AimDownSightStability = weaponData.aimDownSightStability
                };
                weaponDataBuffer.Add(weaponDataBufferElement);
            }
            AddComponent(entity, new EquippedWeaponData
            {
                EquippedWeaponIndex = authoring.equippedWeaponIndex
            });

            //Weapon Hit Result Related
            AddBuffer<WeaponHitResultBufferElement>(entity);

            //VFX Related
            //AddComponent<BulletLineFXTag>(entity);

            //Weapon event related
            AddBuffer<WeaponEventBufferElement>(entity);
        }
    }
}
