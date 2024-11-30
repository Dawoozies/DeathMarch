// this is a singleton class that manages paths - specifically their threading and callbacks.
// It relies on A* Pathfinding Project and entities that have PathData attached
//
// HACK - Because path objects don't have any way to set custom user data on them,
//  there is no easy way of associating an entity with a path in the callback function
//  that gets executed when the path finishes. At first I handled this with a dictionary,
//  which worked, but it was messy trying to keep it synchronized as entities got deleted
//  and added. Since path-penalties are not used in many games, this class uses item 30 and 31
//  of Path.tagPenalties to store the entity ID.
//--------------------------------------------------------------------------------------------------//

using Unity.Mathematics;
using Pathfinding;
using System.Collections.Generic;
using Unity.Entities;

public class PathManager : Singleton<PathManager>
{
    static EntityManager m_em;

    //static readonly List<IPathModifier> DEFAULT_MODS = new List<IPathModifier>()
    //{
    //    new StartEndModifierEx(new StartEndModifierOptions(-1, false, Exactness.ClosestOnNode, Exactness.Original, false, false)),
    //    new FunnelModifierEx(new FunnelModifierOptions(FunnelQuality.High, false, false)),
    //    new RadiusModifierEx(new RadiusModifierOptions(1f)),
    //    new SimpleSmoothModifierEx(new SmoothModifierOptions(SmoothType.Simple, 2, 2, 0.5f, true, 4)),
    //    new ClampToGroundModifierEx(new ClampToGroundModifierOptions(1f), terrainMap)
    //};

    const float PATH_BIAS_MULTIPLIER = 0.04950495049f;

    private void Start()
    {
        m_em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    //// calculates all paths to the specified endpoint to create a FloodPath
    //// calculates a flood path (holds all nodes that connect to destination)
    //public void StartFloodPath(Entity e, float3 endPos)
    //{
    //    FloodPath path = FloodPath.Construct(endPos, OnFloodPathComplete);
    //    path.tagPenalties = new int[32]; // HACK - see note at top of file
    //    path.tagPenalties[0] = 0;
    //    path.tagPenalties[30] = e.Index;
    //    path.tagPenalties[31] = e.Version;
    //    AstarPath.StartPath(path);
    //    //UnityEngine.Debug.Log($"Flood path started for {e.Index}");
    //}

    //// same as StartFloodPath, except this version should be used when endPos is not on the navmesh.
    //// for example, use this when the endpoint is within a navmesh cutter boundary.
    //// radius --> the largest extent (longest distance between centerpoint and edge of the navmesh cutter)
    ////            you can use PhysicsUtils.CalculateRadiusSq(collider) to get this.
    //public void StartFloodPathOffNavmesh(Entity e, float3 startPos, float3 endPos, float radius)
    //{
    //    // if bias is enabled, then offset the point in the direction of the navmesh agent so that the agent doesn't circle around the object.
    //    // see: https://forum.arongranberg.com/t/need-help-understanding-closest-on-node-surface-end-point-snapping/
    //    // and: https://forum.arongranberg.com/t/paths-dont-take-the-direct-route/
    //    endPos += math.normalize(startPos - endPos) * (radius * PATH_BIAS_MULTIPLIER);
    //    StartFloodPath(e, endPos);
    //    //UnityEngine.Debug.Log($"Flood path started for {e.Index}");
    //}

    // calculates a path between start-pos and the destination that was used to calculate floodPath
    public void StartTracerPath(Entity e, float3 startPos, FloodPath floodPath)
    {
        FloodPathTracer path = FloodPathTracer.Construct(startPos, floodPath, OnPathComplete);
        path.tagPenalties = new int[32]; // HACK - see note at top of file
        path.tagPenalties[0] = 0;
        path.tagPenalties[30] = e.Index;
        path.tagPenalties[31] = e.Version;
        AstarPath.StartPath(path);
    }

    // starts calculating an AB path - when finished OnPathComplete() will be called.
    public void StartAbPath(Entity e, float3 startPos, float3 endPos)
    {
        ABPath path = ABPath.Construct(startPos, endPos, OnPathComplete);
        path.tagPenalties = new int[32]; // HACK - see note at top of file
        path.tagPenalties[0] = 0;
        path.tagPenalties[30] = e.Index;
        path.tagPenalties[31] = e.Version;
        AstarPath.StartPath(path);
        //UnityEngine.Debug.Log($"AB path started for {e.Index}");
    }

    public static Path CalculateAbPath(Entity entity, float3 startPos, float3 endPos)
    {
        ABPath path = ABPath.Construct(startPos, endPos, null);
        AstarPath.StartPath(path);
        AstarPath.BlockUntilCalculated(path);
        SetupEntityPath(entity, path);
        return path;
    }

    public FloodPath CalculateFloodPath(float3 endPos)
    {
        FloodPath path = FloodPath.Construct(endPos, null);
        AstarPath.StartPath(path);
        AstarPath.BlockUntilCalculated(path);
        return path;
    }

    public static Path CalculateTracerPath(float3 startPos, FloodPath floodPath)
    {
        FloodPathTracer tracerPath = FloodPathTracer.Construct(startPos, floodPath, null);
        AstarPath.StartPath(tracerPath);
        AstarPath.BlockUntilCalculated(tracerPath);
        //ApplyDefaultStaticMods(tracerPath);
        //ApplyDefaultMods()
        return tracerPath;
    }

    //public static void StartTracerPath(Entity float3 startPos, FloodPath floodPath)
    //{
    //    FloodPathTracer tracerPath = FloodPathTracer.Construct(startPos, floodPath, null);
    //}

    // starts calculating an AB path - when finished OnPathComplete() will be called.
    // radius --> the largest extent (longest distance between centerpoint and edge of the navmesh cutter)
    //            you can use PhysicsUtils.CalculateRadiusSq(collider) to get this.
    public void StartAbPathOffMesh(Entity e, float3 startPos, float3 endPos, float radius)
    {
        // if bias is enabled, then offset the point in the direction of the navmesh agent so that the agent doesn't circle around the object.
        // see: https://forum.arongranberg.com/t/need-help-understanding-closest-on-node-surface-end-point-snapping/
        // and: https://forum.arongranberg.com/t/paths-dont-take-the-direct-route/
        endPos += math.normalize(startPos - endPos) * (radius * PATH_BIAS_MULTIPLIER);
        //UnityEngine.Debug.Log($"Bias total: {radius * PATH_BIAS_MULTIPLIER}, RadiusSq: {radius}");
        StartAbPath(e, startPos, endPos);
    }

    //######################################## PRIVATE ############################################//

    // callback that gets executed when a path that was started via StartPath() or StartTracerPath() finishes calculating
    private static void OnPathComplete(Path newPath)
    {
        // We got our path back
        if (newPath.error)
        {
            UnityEngine.Debug.LogWarning("OnPathComplete > A path couldn't be found.");
            return;
        }

        //EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // HACK - see note at top of file
        Entity entity = new Entity { Index = newPath.tagPenalties[30], Version = newPath.tagPenalties[31] };

        // the entity must exist and not be null
        if (entity == Entity.Null) { return; }
        if (!m_em.Exists(entity)) { return; }

        // the entity can't be dead
        //if (!m_em.HasComponent<LiveTag>(entity)) { return; }

        // apply modifiers and set the new path
        //if (m_mods == null) { m_mods = PathUtils.GetDefaultModifiers(); }
        //List<IPathModifier> mods = PathUtils.GetDefaultModifiers();

        //RadiusData radius = m_em.GetComponentData<RadiusData>(entity);

        TerrainHeightData terrainMap = m_em.CreateEntityQuery(ComponentType.ReadOnly<TerrainHeightData>()).GetSingleton<TerrainHeightData>();

        // note that the order in which the mods are applied is important!
        // also note that some mods are static, while others are dynamic (based on agent radius for example)
        List<IPathModifier> mods = new List<IPathModifier>();
        mods.Add(new StartEndModifierEx(new StartEndModifierOptions(-1, false, Exactness.ClosestOnNode, Exactness.Original, false, false)));
        mods.Add(new FunnelModifierEx(new FunnelModifierOptions(FunnelQuality.High, false, false)));
        mods.Add(new RadiusModifierEx(new RadiusModifierOptions(0.5f)));
        mods.Add(new SimpleSmoothModifierEx(new SmoothModifierOptions(SmoothType.Simple, 2, 2, 0.5f, true, 4)));
        mods.Add(new ClampToGroundModifierEx(new ClampToGroundModifierOptions(0.5f), terrainMap)); // does not do a deep copy because TerrainHeightData contains a BlobAssetReference<>

        //UnityEngine.Debug.Log($"radius: {radius.radius}, heighRadius: {radius.heightRadius}");

        DynamicBuffer<WaypointData> path = m_em.GetBuffer<WaypointData>(entity, false);
        path.Clear();

        DebugDrawUtils.DrawPath(newPath.vectorPath, UnityEngine.Color.white, 10f, 0, -1); // before mods
        for (int i = 0; i < mods.Count; i++) { mods[i].Apply(newPath); }
        DebugDrawUtils.DrawPath(newPath.vectorPath, UnityEngine.Color.yellow, 60f, 0, -1); // after mods

        //DebugDrawUtils.DrawPathWaypoints(newPath.vectorPath, UnityEngine.Color.grey, 60f); // after mods

        // copy the new path into the component
        PathUtils.CopyPath(newPath.vectorPath, ref path);

        // reset the waypoint index to zero and the travel state to default
        WaypointIndexData waypoint = m_em.GetComponentData<WaypointIndexData>(entity);
        waypoint.Index = 0;
        m_em.SetComponentData(entity, waypoint);
        TravelStateData travel = m_em.GetComponentData<TravelStateData>(entity);
        travel.HasReachedEndOfPath = false;
        m_em.SetComponentData(entity, travel);


        /*
        // We got our path back
        if (newPath.error) {
            UnityEngine.Debug.LogWarning("OnPathComplete > A path couldn't be found.");
            return;
        }

        //EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // HACK - see note at top of file
        Entity entity = new Entity { Index = newPath.tagPenalties[30], Version = newPath.tagPenalties[31] };

        // the entity must exist and not be null
        if (!m_em.Exists(entity)) { UnityEngine.Debug.Log("OnPathComplete > Tried to set path for a non-existant entity."); return; }
        if (entity == Entity.Null) { UnityEngine.Debug.Log("OnPathComplete > Tried to set path for a null entity."); return; }

        // apply modifiers and set the new path
        //if (m_mods == null) { m_mods = PathUtils.GetDefaultModifiers(); }
        //List<IPathModifier> mods = PathUtils.GetDefaultModifiers();

        //ApplyDefaultMods(newPath, new RadiusData { heightRadius = 1f, xzRadius = 1f });

        // should use precalculated radius data from entity
        SetupEntityPath(entity, newPath, new RadiusData { heightRadius = 1f, xzRadius = 1f });

        //UnityEngine.Debug.Log($"Path complete for {entity.Index} of size {newPath.path.Count}");
        */
    }

    public static void ApplyDefaultMods(Path path, float radius, float height, TerrainHeightData terrainMap)
    {        
        new StartEndModifierEx(new StartEndModifierOptions(-1, false, Exactness.ClosestOnNode, Exactness.Original, false, false)).Apply(path);
        new FunnelModifierEx(new FunnelModifierOptions(FunnelQuality.High, false, false)).Apply(path);
        new RadiusModifierEx(new RadiusModifierOptions(radius)).Apply(path);
        new SimpleSmoothModifierEx(new SmoothModifierOptions(SmoothType.Simple, 2, 2, 0.5f, true, 4)).Apply(path);
        new ClampToGroundModifierEx(new ClampToGroundModifierOptions(height), terrainMap).Apply(path);
    }

    //public static void ApplyDefaultStaticMods(Path path)
    //{
    //    for (int i = 0; i < DEFAULT_MODS.Count; i++) { DEFAULT_MODS[i].Apply(path); }
    //}

    static void SetupEntityPath(Entity entity, Path newPath)
    {
        DynamicBuffer<WaypointData> path = m_em.GetBuffer<WaypointData>(entity, false);
        path.Clear();

        //DebugDrawUtils.DrawPath(newPath.vectorPath, UnityEngine.Color.white, 10f); // before mods
        //ApplyDefaultStaticMods(newPath);
        //DebugDrawUtils.DrawPath(newPath.vectorPath, UnityEngine.Color.yellow, 60f); // after mods
        //DebugDrawUtils.DrawPathWaypoints(newPath.vectorPath, UnityEngine.Color.grey, 60f); // after mods

        // copy the new path into the component
        PathUtils.CopyPath(newPath.vectorPath, ref path);

        // reset the waypoint index to zero and the travel state to default
        WaypointIndexData waypoint = m_em.GetComponentData<WaypointIndexData>(entity);
        waypoint.Index = 0;
        m_em.SetComponentData(entity, waypoint);

        TravelStateData travel = m_em.GetComponentData<TravelStateData>(entity);
        travel.HasReachedEndOfPath = false;
        m_em.SetComponentData(entity, travel);

        // enable movement
        m_em.AddComponent<EnableMovementTag>(entity);
    }

    //// this gets called whenever a flood path calculation completes
    //private void OnFloodPathComplete(Path newPath)
    //{
    //    if (newPath.error) {
    //        UnityEngine.Debug.LogWarning("OnFloodPathComplete > A flood path couldn't be found.");
    //        return;
    //    }

    //    // HACK - see note at top of file
    //    Entity entity = new Entity { Index = newPath.tagPenalties[30], Version = newPath.tagPenalties[31] };

    //    // the entity must exist and not be null
    //    if (!m_em.Exists(entity) || (entity == Entity.Null)) { return; }

    //    // the entity can't be dead
    //    //if (!m_em.HasComponent<LiveTag>(entity)) { UnityEngine.Debug.Log("tried to set path for a dead bug"); return; }

    //    LegacyPathData legacy = m_em.GetComponentData<LegacyPathData>(entity);
    //    legacy.floodPath = (FloodPath)newPath;
    //    m_em.SetComponentData(entity, legacy);
    //}
}