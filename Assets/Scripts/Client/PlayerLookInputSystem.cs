using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
using Unity.Physics;
using UnityEngine.InputSystem;
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerLookInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
    private CollisionFilter _selectionFilter;
    private float2 mousePositionDelta;
    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
        _selectionFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5, //Raycasts
            CollidesWith = 1 << 0 //GroundPlane
        };
        RequireForUpdate<OwnerChampTag>();
    }
    protected override void OnStartRunning()
    {
        _inputActions.Enable();
    }
    protected override void OnStopRunning()
    {
        _inputActions.Disable();
    }
    protected override void OnUpdate()
    {
        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        Entity cameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
        Camera mainCamera = EntityManager.GetComponentObject<MainCamera>(cameraEntity).Value;

        float3 cameraForward = mainCamera.transform.forward;
        float3 cameraRight = mainCamera.transform.right;
        float3 cameraUp = mainCamera.transform.up;

        RaycastInput playerLookCast = new RaycastInput
        {
            Start = mainCamera.transform.position,
            End = mainCamera.transform.position + mainCamera.transform.forward * 100f,
            Filter = _selectionFilter
        };

        Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
        if(collisionWorld.CastRay(playerLookCast, out var closestHit))
        {
            EntityManager.SetComponentData(playerEntity, new PlayerLookInput
            {
                Value = closestHit.Position
            });
        }
        EntityManager.SetComponentData(playerEntity, new PlayerCameraDirections 
        {
            Forward = cameraForward,
            Right = cameraRight,
            Up = cameraUp
        });
    }
}