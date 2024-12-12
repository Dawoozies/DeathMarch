// ensures that the terrain height data singleton exists

using Unity.Burst;
using Unity.Entities;
using UnityEngine;
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial class TerrainInitSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (SystemAPI.TryGetSingleton<TerrainHeightData>(out TerrainHeightData thd)) { return; }
        Terrain terrain = GameObject.FindFirstObjectByType<Terrain>();
        bool terrainNull = terrain == null;
        //if (terrain == null) { UnityEngine.Debug.Log($"Couldn't find terrain in scene!"); }
        Entity entity = EntityManager.CreateEntity();
        if(!terrainNull)
        {
            EntityManager.AddComponentData(entity, new TerrainHeightData(terrain));
        }
    }
}