using Unity.Cinemachine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEngine.InputSystem;
namespace TMG.NFE_Tutorial
{
    public class CameraController : MonoBehaviour
    {
        public CinemachineCamera firstPersonCamera;
        private EntityManager _entityManager;
        private EntityQuery _localChampQuery;
        private bool _cameraSet;
        public Entity localChamp;
        Camera mainCamera;
        public float maxLookDistance;
        public LayerMask mouseCastLayers;
        public float smoothTime;
        public float defaultSmoothTime;
        Vector3 pos_v;
        InputSystem_Actions inputActions;
        Vector2 mousePositionDelta;
        Vector2 angles;
        public Vector2 yRotLimit;
        public Vector3 posOffset;
        public float mouseSensitivity;
        Vector3 normalPos;
        Vector3 aimDownSightPos;
        public float aimDownSightSpeed;
        float aimDownSightValue;
        public float normalFOV;
        public float aimDownSightFOV;
        private void Awake()
        {
            inputActions = new InputSystem_Actions();
            mainCamera = Camera.main;
        }
        void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.MousePosition.performed += InputMouseDelta;
        }
        void OnDisable()
        {
            inputActions.Player.MousePosition.performed -= InputMouseDelta;
            inputActions.Disable();
        }
        private void Start()
        {
            firstPersonCamera = GetComponentInChildren<CinemachineCamera>();
            if (World.DefaultGameObjectInjectionWorld == null) return;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _localChampQuery = _entityManager.CreateEntityQuery(typeof(OwnerChampTag));
        }
        private void InputMouseDelta(InputAction.CallbackContext context)
        {
            mousePositionDelta = context.ReadValue<Vector2>();
            angles += mousePositionDelta * Time.deltaTime * mouseSensitivity;
            angles.y = Mathf.Clamp(angles.y, yRotLimit.x, yRotLimit.y);
            Quaternion xRot = Quaternion.AngleAxis(angles.x, Vector3.up);
            Quaternion yRot = Quaternion.AngleAxis(angles.y, Vector3.left);
            transform.localRotation = xRot * yRot;
        }
        private void Update()
        {
            SetCamera();

            if(!_cameraSet)
                return;
            FollowTargetPlayer();
            AimDownSight();
            PlayerAimInput aimInput = _entityManager.GetComponentData<PlayerAimInput>(localChamp);
            if(aimInput.Value)
            {
                aimDownSightValue += Time.deltaTime * aimDownSightSpeed;
            }
            else
            {
                aimDownSightValue -= Time.deltaTime * aimDownSightSpeed;
            }
            aimDownSightValue = Mathf.Clamp01(aimDownSightValue);
            Vector3 targetPos = Vector3.Lerp(normalPos, aimDownSightPos, aimDownSightValue);
            firstPersonCamera.Lens.FieldOfView = Mathf.Lerp(normalFOV, aimDownSightFOV, aimDownSightValue);
            smoothTime = aimDownSightValue > 0 ? 0f : defaultSmoothTime;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref pos_v, smoothTime);
        }
        void AimDownSight()
        {
            var equippedWeaponData = _entityManager.GetComponentData<EquippedWeaponData>(localChamp);
            var weaponDataBuffer = _entityManager.GetBuffer<WeaponDataBufferElement>(localChamp);
            WeaponDataBufferElement weaponData = weaponDataBuffer[equippedWeaponData.EquippedWeaponIndex];
            LocalToWorld ADSWorldPos = _entityManager.GetComponentData<LocalToWorld>(weaponData.WeaponAimDownSightPosition);
            aimDownSightPos = ADSWorldPos.Position;
        }
        void FollowTargetPlayer()
        {
            var localTransform = _entityManager.GetComponentData<LocalTransform>(localChamp);
            normalPos = localTransform.Position + (float3)posOffset;
        }
        private void SetCamera()
        {
            if (!_cameraSet)
            {
                if (_localChampQuery.TryGetSingletonEntity<OwnerChampTag>(out var localChamp))
                {
                    this.localChamp = localChamp;
                    var playerTransform = _entityManager.GetComponentData<LocalTransform>(localChamp);
                    transform.position = playerTransform.Position;
                    _cameraSet = true;
                }
            }
        }
    }
}