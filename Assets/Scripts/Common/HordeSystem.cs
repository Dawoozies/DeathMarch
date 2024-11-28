using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;
using UnityEngine;
using Pathfinding.ECS;

[BurstCompile]
public partial struct HordeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ChampTag>();
        state.RequireForUpdate<SimulationSingleton>();
    }
    public void OnUpdate(ref SystemState state)
    {
        NativeList<LocalTransform> playerTransforms = new NativeList<LocalTransform>(Allocator.Temp);
        foreach (var playerTransform in SystemAPI.Query<LocalTransform>().WithAll<Simulate, ChampTag>())
        {
            playerTransforms.Add(playerTransform);
        }
        if (playerTransforms.Length <= 0)
            return;
        int entityCount = 0;
        foreach (var (transform, hordeMove, destinationPoint) in SystemAPI.Query<
        LocalTransform,
        HordeMove,
        RefRW<DestinationPoint>
        >().WithAll<Simulate>())
        {
            entityCount++;
            LocalTransform target = GetClosestPlayer(playerTransforms, transform.Position);
            destinationPoint.ValueRW.destination = target.Position;
        }
        //put code for path finding in here
        Debug.Log($"HordeCount={entityCount}");
        playerTransforms.Dispose();
    }
    public LocalTransform GetClosestPlayer(NativeList<LocalTransform> players, float3 agentPos)
    {
        float closestDist = math.INFINITY;
        int closestPlayer = 0;
        if (players.Length == 1)
        {
            return players[closestPlayer];
        }
        for (int i = 1; i < players.Length; i++)
        {
            float3 playerPos = players[i].Position;
            float d = math.distancesq(agentPos, playerPos);
            if (d < closestDist)
            {
                closestDist = d;
                closestPlayer = i;
            }
        }
        return players[closestPlayer];
    }
}