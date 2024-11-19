using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
using Unity.Physics;
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerLookInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;
    private CollisionFilter _selectionFilter;
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

        float2 cameraForward = new float2(mainCamera.transform.forward.x, mainCamera.transform.forward.z);
        float2 cameraRight = new float2(mainCamera.transform.right.x, mainCamera.transform.right.z);
        cameraForward = math.normalizesafe(cameraForward);
        cameraRight = math.normalizesafe(cameraRight);

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 100f;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        RaycastInput selectionInput = new RaycastInput
        {
            Start = mainCamera.transform.position,
            End = worldPosition,
            Filter = _selectionFilter
        };

        Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
        if(collisionWorld.CastRay(selectionInput, out var closestHit))
        {
            EntityManager.SetComponentData(playerEntity, new PlayerLookInput
            {
                Value = closestHit.Position,
            });
        }
        EntityManager.SetComponentData(playerEntity, new PlayerCameraDirections 
        {
            Forward = cameraForward,
            Right = cameraRight
        });
    }
}