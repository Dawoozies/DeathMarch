// this is a static utility class that contains useful pathfinding functions.
// It relies on A* Pathfinding Project and entities that have path data attached
//--------------------------------------------------------------------------------------------------//

using Unity.Mathematics;
using Pathfinding;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct WaypointSelectResult
{
    public float DistanceSq;
    public int CurrentIndex;
    public bool HasReachedEndOfPath;
}

public static class PathUtils
{
    public static WaypointSelectResult SelectWaypoint(float3 pos, float desiredDistance, int currentWaypointIndex, in DynamicBuffer<WaypointData> path)
    {
        float desiredDistSq = desiredDistance * desiredDistance;
        float curDistSq = 0f;
        int curIdx = currentWaypointIndex;
        bool hasReachedEndOfPath = false;
        while (true) {
            curDistSq = math.distancesq(pos, path[curIdx].Point);
            int nextIdx = curIdx + 1;
            if (curDistSq < desiredDistSq) {
                // this waypoint is close enough that we can ignore it and look for a better one.
                // Check if there is another waypoint or if we have reached the end of the path
                if (nextIdx < path.Length) {
                    curIdx++;
                } else {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    hasReachedEndOfPath = true;
                    break;
                }
            } else if (nextIdx < path.Length) {
                // this current waypoint is too far away to reach. see if the one after it is any closer.
                // if it is, then choose that as the current waypoint instead.
                float nextDistSq = math.distancesq(pos, path[nextIdx].Point);
                if (nextDistSq < curDistSq) {
                    // this next point is closer than the current waypoint
                    // so skip the current waypoint (even though we never reached it)
                    // and make this new point the current waypoint
                    curIdx = nextIdx;
                    curDistSq = nextDistSq;
                } else {
                    break; // current waypoint is closest
                }
            } else {
                break;
            }
        }

        WaypointSelectResult result;
        result.CurrentIndex = curIdx;
        result.HasReachedEndOfPath = hasReachedEndOfPath;
        result.DistanceSq = curDistSq;

        return result;
    }

    // calculates a path between start and end and copies it from A*PFP's vectorPath to a FixedList4096
    public static void CalculateAbPath(ref DynamicBuffer<WaypointData> destPath, float3 startPos, float3 endPos, List<IPathModifier> mods)
    {
        ABPath path = ABPath.Construct(startPos, endPos, null);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();
        for (int i = 0; i < mods.Count; i++) { mods[i].Apply(path); }
        CopyPath(path.vectorPath, ref destPath);
    }

    // calculates a flood path (holds all nodes that connect to destination)
    // this function blocks until the floodpath is calculated
    // returns a reference to the floodpath class
    public static FloodPath CalculateFloodPath(float3 endPos)
    {
        FloodPath floodPath = FloodPath.Construct(endPos, null);
        AstarPath.StartPath(floodPath);
        floodPath.BlockUntilCalculated();
        return floodPath;
    }

    // calculates a path between start-pos and the destination that was used to calculate floodPath
    public static void CalculateTracerPath(Entity e, float3 startPos, FloodPath floodPath, List<IPathModifier> mods)
    {
        FloodPathTracer tracerPath = FloodPathTracer.Construct(startPos, floodPath, null);
        AstarPath.StartPath(tracerPath);
        AstarPath.BlockUntilCalculated(tracerPath);
    }

    public static void CopyPath(List<Vector3> src, ref DynamicBuffer<WaypointData> dest)
    {
        dest.Clear();
        for (int i = 0; i < src.Count; i++) {
            dest.Add(new WaypointData { Point = src[i] });
        }
    }

    static public void CopyPathAndReset(EntityManager em, List<Vector3> srcPath, Entity destEntity)
    {
        // path buffer
        DynamicBuffer<WaypointData> destPath = em.GetBuffer<WaypointData>(destEntity, false);
        CopyPath(srcPath, ref destPath);

        // reset waypoint index
        em.SetComponentData(destEntity, new WaypointIndexData { Index = 0 });

        // reset travel state
        em.SetComponentData(destEntity, new TravelStateData { HasReachedEndOfPath = false });

        // add enable movement tag
        em.AddComponent<EnableMovementTag>(destEntity);
    }
}