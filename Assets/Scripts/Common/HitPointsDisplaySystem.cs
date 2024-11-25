using Unity.Entities;
using Unity.Transforms;
using TMPro;
using UnityEngine;
[UpdateAfter(typeof(TransformSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct HealthDisplaySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<UIPrefabs>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        Entity cameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
        Camera mainCamera = state.EntityManager.GetComponentObject<MainCamera>(cameraEntity).Value;
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (transform, offset, maxHealth, entity) in SystemAPI.Query<
        LocalTransform, 
        HealthUIOffset, 
        MaxHitPoints
        >().WithNone<HealthUIReference>().WithEntityAccess())
        {
            var healthPrefab = SystemAPI.ManagedAPI.GetSingleton<UIPrefabs>().HealthDisplay;
            var spawnPosition = transform.Position + offset.Value;
            var newHealthDisplay = Object.Instantiate(healthPrefab, spawnPosition, Quaternion.identity);
            SetHealth(newHealthDisplay, maxHealth.Value, maxHealth.Value);
            newHealthDisplay.transform.SetParent(WorldCanvasSingleton.ins.transform, true);
            ecb.AddComponent(entity, new HealthUIReference{Value = newHealthDisplay});
        }

        foreach (var (transform, offset, currentHealth, maxHealth, healthDisplay) in SystemAPI.Query<
        LocalTransform, 
        HealthUIOffset, 
        CurrentHitPoints,
        MaxHitPoints,
        HealthUIReference
        >())
        {
            var healthDisplayPos = transform.Position + offset.Value;
            healthDisplay.Value.transform.position = healthDisplayPos;
            healthDisplay.Value.transform.forward = -(mainCamera.transform.position - (Vector3)healthDisplayPos);
            SetHealth(healthDisplay.Value, currentHealth.Value, maxHealth.Value);
        }
    }
    private void SetHealth(GameObject healthObject, int currentHealth, int maxHealth)
    {
        var healthTextMesh = healthObject.GetComponent<TMP_Text>();
        healthTextMesh.text = $"HP={currentHealth}/{maxHealth}";
    }
}