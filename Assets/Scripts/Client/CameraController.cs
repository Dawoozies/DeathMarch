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
        private EntityManager _entityManager;
        private EntityQuery _localChampQuery;
        private bool _cameraSet;
        public Entity localChamp;
        Camera mainCamera;
        public float maxLookDistance;
        public LayerMask mouseCastLayers;
        public float smoothTime;
        Vector3 pos_v;
        InputSystem_Actions inputActions;
        Vector2 mousePositionDelta;
        Vector2 angles;
        public Vector2 yRotLimit;
        public Vector3 posOffset;
        public float mouseSensitivity;
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
            
        }
        void FollowTargetPlayer()
        {
            var localTransform = _entityManager.GetComponentData<LocalTransform>(localChamp);
            //Debug.Log($"localChamp.Position = {localTransform.Position}");
            transform.position = Vector3.SmoothDamp(transform.position, localTransform.Position + (float3)posOffset, ref pos_v, smoothTime);
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