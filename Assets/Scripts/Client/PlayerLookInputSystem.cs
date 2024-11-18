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

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 100f;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        RaycastInput selectionInput = new RaycastInput
        {
            Start = mainCamera.transform.position,
            End = worldPosition,
            Filter = _selectionFilter
        };

        if(collisionWorld.CastRay(selectionInput, out var closestHit))
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
            EntityManager.SetComponentData(playerEntity, new PlayerLookInput
            {
                Value = closestHit.Position
            });
        }
    }
}