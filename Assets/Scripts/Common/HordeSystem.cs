using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;
using UnityEngine;
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
        foreach (var (transform, physicsVelocity, hordeMove, groundCheck) in SystemAPI.Query<
        RefRW<LocalTransform>,
        RefRW<PhysicsVelocity>,
        HordeMove,
        GroundCheck
        >().WithAll<Simulate>())
        {
            entityCount++;
            float3 targetPos = GetClosestPlayer(playerTransforms, transform.ValueRO.Position).Position;
            float3 v = math.normalizesafe(targetPos - transform.ValueRO.Position)*hordeMove.MoveSpeed;
            if(groundCheck.AirTime <= 0.1f)
            {
                v.y = 1f;
            }
            else
            {
                v.y -= hordeMove.Gravity;
            }
            physicsVelocity.ValueRW.Linear = v;
            float3 fwd = v;
            fwd.y = 0f;
            transform.ValueRW.Rotation = quaternion.LookRotationSafe(fwd, math.up());
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