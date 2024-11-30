using Unity.Transforms;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public partial struct RvoPathMovementJob : IJobEntity
{
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public TerrainHeightData TerrainMap;
    [ReadOnly] public NativeArray<float3>.ReadOnly SimOutDataTargetPoints;
    [ReadOnly] public NativeArray<float>.ReadOnly SimOutDataSpeeds;

    [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<float3> SimDataPositions;
    [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<float3> SimDataTargetPoints;
    [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<float> SimDataDesiredSpeeds;
    [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<float> SimDataMaxSpeeds;
    [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<float3> SimDataEndOfPaths;

    // Loops through all agents and sets their next position (pos.Value),
    // and at the same time it updates the RVO simulation for each agent.
    [BurstCompile]
    public void Execute(ref LocalTransform lclXform, ref WaypointIndexData waypoint, ref TravelStateData travel, in MoveSpeedData speed, in DynamicBuffer<WaypointData> path, in RvoAgentData rvoAgent)
    {
        if (path.Length <= 0) { return; }
        if (travel.HasReachedEndOfPath) { return; }
        //if (dest.TargetEntity == Entity.Null) { return; }

        int agentIndex = rvoAgent.agentIndex;

        WaypointSelectResult result = PathUtils.SelectWaypoint(lclXform.Position, 6f, waypoint.Index, path);

        // only set the target again if the next waypoint has changed
        if ((waypoint.Index == 0) || (waypoint.Index != result.CurrentIndex))
        {
            // set the new waypoint index
            waypoint.Index = result.CurrentIndex;
            travel.HasReachedEndOfPath = result.HasReachedEndOfPath;
            // if this is the end of the path, then we should return (the code below wont make sense because the target waypoint
            // will be very close or the same as the current position and that will give a bad direction.
            if (travel.HasReachedEndOfPath) { return; }
            // fetch the position for the current waypoint
            float3 curWaypointPos = path[waypoint.Index].Point;

            // Ideally, I would use SimData.SetTarget(), but that does not work because the compiler will freak out about writing to SimData.
            // So instead, I had to look at the source code to SetTarget() and notice that it first clamps the max and desired speeds,
            // and then after it sets these fields (all are arrays):
            //    SimData.targetPoint
            //    SimData.desiredSpeed
            //    SimData.maxSpeed
            //    SimData.endOfPath
            // I made NativeArray pointers to each one, and then made sure that they were declared
            // with [WriteOnly] and [NativeDisableParallelForRestriction]


            float maxSpeed = math.max(speed.Speed * 1.4f, 0);
            float desiredSpeed = math.clamp(speed.Speed, 0, maxSpeed);

            SimDataTargetPoints[agentIndex] = curWaypointPos;
            SimDataDesiredSpeeds[agentIndex] = desiredSpeed;
            SimDataMaxSpeeds[agentIndex] = maxSpeed;
            SimDataEndOfPaths[agentIndex] = path[path.Length - 1].Point;

            //DebugDrawUtils.DrawCross(curWaypointPos, 0.8f, UnityEngine.Color.white, 5f);
        }

        // This is the point and speed that the RVO agent should move to, as dicatated by the RVO sim.
        // If it's the only agent around, then this point will likely be the same as the current waypoint position.
        // However, if there are a lot of other agents or obstacles in the vicinity, then this point will likely
        // be different so that the agent can avoid bumping into other agents or obstacles.
        // Note that this point has an incorrect height (y value) that doesn't match the terrain. I don't know why.
        // This point is the same as rvoAgent.CalculatedTargetPoint.
        float3 targetPt = SimOutDataTargetPoints[agentIndex];

        if (MathUtils.IsZero(targetPt)) { return; }

        // Fetch the desired speed from the sim output (same as rvoAgent.CalculatedSpeed)
        float calculatedSpeed = SimOutDataSpeeds[agentIndex];

        // Because targetPt has an incorrect height (not sure why), I must sample the appropriate height from the terrain,
        // and set its target point's y value to that instead.
        targetPt.y = TerrainMap.SampleHeight(targetPt) + 1f;

        // if the target point and the current position are zero, it means the sim wants the agent to stay put.
        // exit now because there's nothing left to do, and also because the below look-rotation would give a NaN.
        if (MathUtils.Approximately(targetPt, lclXform.Position, 0.0001f)) { return; }

        // Calculate the direction to the next calculated target point.
        float3 dir = math.normalize(targetPt - lclXform.Position);

        // Set the entity's rotation to face in the direction of movement.
        lclXform.Rotation = quaternion.LookRotation(dir, math.up());

        // Move the entity in the necessary direction by the amount at delta-time multiplied with the calculated speed.
        lclXform.Position += (dir * DeltaTime * calculatedSpeed);

        //// If the height of the entity is way above or way below the terrain, teleport it to terrain level.
        //// This is needed because for some reason a small percentage of entities randomly
        //// end up walking through the air or below ground, especially ones that missed a waypoint.
        //float heightAtCurPos = TerrainMap.SampleHeight(lclXform.Position);
        //if (math.abs(heightAtCurPos - lclXform.Position.y) > 1f) { lclXform.Position.y = heightAtCurPos + 1f; }

        // The sim's position must be updated as well (it should mirror the entity's position)
        // so that the sim knows what the entity's current position is.
        SimDataPositions[agentIndex] = lclXform.Position;
    }
}

// systembase must be used instead of isystem because m_simBurst is a managed object
[RequireMatchingQueriesForUpdate]
public partial class RvoPathMovementSystem : SystemBase
{
    Pathfinding.RVO.SimulatorBurst m_simBurst;

    protected override void OnStartRunning()
    {
        RequireForUpdate<TerrainHeightData>();
        m_simBurst = UnityEngine.GameObject.Find("RvoSimulator").GetComponent<Pathfinding.RVO.RVOSimulator>().GetSimulator();
    }

    protected override void OnUpdate()
    {
        RvoPathMovementJob job = new RvoPathMovementJob();
        job.DeltaTime = SystemAPI.Time.DeltaTime;
        job.SimOutDataTargetPoints = m_simBurst.outputData.targetPoint.AsReadOnly();
        job.SimOutDataSpeeds = m_simBurst.outputData.speed.AsReadOnly();
        job.TerrainMap = SystemAPI.GetSingleton<TerrainHeightData>();

        job.SimDataDesiredSpeeds = m_simBurst.simulationData.desiredSpeed;
        job.SimDataEndOfPaths = m_simBurst.simulationData.endOfPath;
        job.SimDataPositions = m_simBurst.simulationData.position;
        job.SimDataMaxSpeeds = m_simBurst.simulationData.maxSpeed;
        job.SimDataTargetPoints = m_simBurst.simulationData.targetPoint;

        // because I'm no longer using SimData.SetTarget() and I now access the RVO sim via arrays with NativeDisableParallelForRestriction,
        // I can use ScheduleParallel() instead of just Schedule()!
        job.ScheduleParallel();
    }
}