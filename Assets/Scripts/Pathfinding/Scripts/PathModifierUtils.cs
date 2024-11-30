// This static class contains functions useful for pathfinding.
// It also contains definitions for modifier classes that inherit IPathModifier and options structures.
// It relies on A* Pathfinding Project, and nearly all the code comes from it - it's just reorganized into an API 
// that doesn't rely on classes and gameobjects.
//--------------------------------------------------------------------------------------------------//

using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Util;
using Pathfinding.Pooling;

public enum SmoothType { Simple, Bezier, OffsetSimple, CurvedNonuniform }
public enum FunnelQuality { Medium, High }
public enum Exactness { SnapToNode, Original, Interpolate, ClosestOnNode, NodeConnection }

public class ClampToGroundModifierEx : IPathModifier
{
    ClampToGroundModifierOptions m_opts;
    TerrainHeightData m_terrainMap;

    public int Order { get { return 60; } }
    public void Apply(Path p) { PathModifierUtils.ApplyClampToGroundModifier(m_opts, p, m_terrainMap); }
    public void PreProcess(Path path) { }
    public ClampToGroundModifierEx(ClampToGroundModifierOptions opts, in TerrainHeightData terrainMap)
    {
        m_opts = opts;
        m_terrainMap = terrainMap;
    }
}

public class StartEndModifierEx : IPathModifier
{
    StartEndModifierOptions m_opts;
    List<GraphNode> m_connectionBuffer;
    System.Action<GraphNode> m_connectionBufferAddDelegate;

    public int Order { get { return 0; } }
    public void Apply(Path p) { PathModifierUtils.ApplyStartEndModifier(m_opts, p, ref m_connectionBuffer, ref m_connectionBufferAddDelegate); }
    public void PreProcess(Path path) { }
    public StartEndModifierEx() { m_opts = default; }
    public StartEndModifierEx(StartEndModifierOptions opts) { m_opts = opts; }
}

public class FunnelModifierEx : IPathModifier
{
    FunnelModifierOptions m_opts;

    public int Order { get { return 10; } }
    public void Apply(Path p) { PathModifierUtils.ApplyFunnelModifier(m_opts, p); }
    public void PreProcess(Path path) { }
    public FunnelModifierEx() { m_opts = default; }
    public FunnelModifierEx(FunnelModifierOptions opts) { m_opts = opts; }
}

public class RadiusModifierEx : IPathModifier
{
    RadiusModifierOptions m_opts;
    RadiusArrays m_arrays = new RadiusArrays();

    public int Order { get { return 41; } }
    public void Apply(Path p) { PathModifierUtils.ApplyRadiusModifier(m_opts, p, m_arrays); }
    public void PreProcess(Path path) { }
    public RadiusModifierEx() { m_opts = default; }
    public RadiusModifierEx(RadiusModifierOptions opts) { m_opts = opts; }
}

public class SimpleSmoothModifierEx : IPathModifier
{
    SmoothModifierOptions m_opts;

    public int Order { get { return 50; } }
    public void Apply(Path p) { PathModifierUtils.ApplySmoothModifier(m_opts, p); }
    public void PreProcess(Path path) { }

    public SimpleSmoothModifierEx() { m_opts = default; }
    public SimpleSmoothModifierEx(SmoothModifierOptions opts) { m_opts = opts; }
}

[System.Serializable]
public struct FunnelModifierOptions
{
    public FunnelQuality quality; // FunnelQuality.Medium
    public bool splitAtEveryPortal; // false
    public bool accountForGridPenalties;
    public FunnelModifierOptions(FunnelQuality q = FunnelQuality.Medium, bool split = false, bool useGridPenalties = false)
    {
        quality = q;
        splitAtEveryPortal = split;
        accountForGridPenalties = useGridPenalties;
    }
}

[System.Serializable]
public struct RadiusModifierOptions
{
    public float radius; // 1f;
    public float detail; // 10f;
    public RadiusModifierOptions(float r = 1f, float d = 10f)
    {
        radius = r;
        detail = d;
    }
}

[System.Serializable]
public class RadiusArrays
{
    public float[] radi = new float[10];
    public float[] a1 = new float[10];
    public float[] a2 = new float[10];
    public bool[] dir = new bool[10];
}

[System.Serializable]
public struct SmoothModifierOptions
{
    public SmoothType smoothType; // SmoothType.Simple
    public int subdivisions; // 2
    public int iterations; // 2
    public float strength; // 0.5f
    public bool uniformLength; // true
    public float maxSegmentLength; // 2f
    public float bezierTangentLength; // 0.4f
    public float offset; // 0.2f
    public float factor; // 0.1f
    public SmoothModifierOptions(SmoothType st = SmoothType.Simple, int sd = 2, int it = 2, float str = 0.5f, bool ul = true, float msl = 2f, float btl = 0.4f, float off = 0.2f, float fact = 0.1f)
    {
        smoothType = st;
        subdivisions = sd;
        iterations = it;
        strength = str;
        uniformLength = ul;
        maxSegmentLength = msl;
        bezierTangentLength = btl;
        offset = off;
        factor = fact;
    }
}

[System.Serializable]
public struct ClampToGroundModifierOptions
{
    public float offset; // 0f
    public ClampToGroundModifierOptions(float off = 0f)
    {
        offset = off;
    }
}

[System.Serializable]
public struct StartEndModifierOptions
{
    public bool addPoints; // false
    public Exactness exactStartPoint;// = Exactness.ClosestOnNode;
    public Exactness exactEndPoint;// = Exactness.ClosestOnNode;
    public bool useRaycasting;// false
    public LayerMask mask;// = -1;
    public bool useGraphRaycasting; // false
    public StartEndModifierOptions(LayerMask msk, bool ap = false, Exactness esp = Exactness.ClosestOnNode, Exactness eep = Exactness.ClosestOnNode, bool ur = false, bool ugr = false)
    {
        mask = msk;
        addPoints = ap;
        exactStartPoint = esp;
        exactEndPoint = eep;
        useRaycasting = ur;
        mask = msk;
        useGraphRaycasting = ugr;
    }
}

public static class PathModifierUtils
{
    public static Vector3 ClampToNavmesh2d(Vector3 position, in IMovementPlane plane, out bool positionChanged, ref Vector2 velocity2D)
    {
        NNConstraint cachedNNConstraint = NNConstraint.Walkable;
        //cachedNNConstraint.tags = seeker.traversableTags;
        cachedNNConstraint.tags = -1;
        //cachedNNConstraint.graphMask = seeker.graphMask;
        cachedNNConstraint.graphMask = GraphMask.everything;
        cachedNNConstraint.distanceMetric = DistanceMetric.ClosestAsSeenFromAbove();
        var clampedPosition = AstarPath.active.GetNearest(position, cachedNNConstraint).position;

        // We cannot simply check for equality because some precision may be lost
        // if any coordinate transformations are used.
        var difference = plane.ToPlane(clampedPosition - position);
        float sqrDifference = difference.sqrMagnitude;
        if (sqrDifference > 0.001f * 0.001f) {
            // The agent was outside the navmesh. Remove that component of the velocity
            // so that the velocity only goes along the direction of the wall, not into it
            velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

            positionChanged = true;
            // Return the new position, but ignore any changes in the y coordinate from the ClampToNavmesh method as the y coordinates in the navmesh are rarely very accurate
            return position + plane.ToWorld(difference);
        }

        positionChanged = false;
        return position;
    }

    public static void ApplySmoothModifier(in SmoothModifierOptions opts, Path p)
    {
        List<Vector3> smoothPath = null;
        switch (opts.smoothType) {
            case SmoothType.Simple: smoothPath = SmoothSimple(p.vectorPath, opts.uniformLength, opts.maxSegmentLength, opts.subdivisions, opts.strength, opts.iterations); break;
            case SmoothType.Bezier: smoothPath = SmoothBezier(p.vectorPath, opts.subdivisions, opts.bezierTangentLength); break;
            case SmoothType.OffsetSimple: smoothPath = SmoothOffsetSimple(p.vectorPath, opts.iterations, opts.offset); break;
            case SmoothType.CurvedNonuniform: smoothPath = CurvedNonuniform(p.vectorPath, opts.maxSegmentLength, opts.factor); break;
        }
        if (smoothPath != p.vectorPath) {
            ListPool<Vector3>.Release(ref p.vectorPath);
            p.vectorPath = smoothPath;
        }
    }

    public static void ApplyFunnelModifier(in FunnelModifierOptions opts, Path p)
    {
        if (p.path == null || p.path.Count == 0 || p.vectorPath == null || p.vectorPath.Count == 0) { return; }

        List<Vector3> funnelPath = ListPool<Vector3>.Claim();

        // Split the path into different parts (separated by custom links)
        // and run the funnel algorithm on each of them in turn
        var parts = Funnel.SplitIntoParts(p);

        if (parts.Count == 0) {
            // As a really special case, it might happen that the path contained only a single node
            // and that node was part of a custom link (e.g added by the NodeLink2 component).
            // In that case the SplitIntoParts method will not know what to do with it because it is
            // neither a link (as only 1 of the 2 nodes of the link was part of the path) nor a normal
            // path part. So it will skip it. This will cause it to return an empty list.
            // In that case we want to simply keep the original path, which is just a single point.
            return;
        }

        if (opts.quality == FunnelQuality.High) Funnel.Simplify(parts, ref p.path);

        for (int i = 0; i < parts.Count; i++) {
            var part = parts[i];
            if (part.type == Funnel.PartType.NodeSequence) {
                // If this is a grid graph (and not a hexagonal graph) then we can use a special
                // string pulling algorithm for grid graphs which works a lot better.
                if (p.path[part.startIndex].Graph is GridGraph gg && gg.neighbours != NumNeighbours.Six) {
                    // TODO: Avoid dynamic allocations
                    System.Func<GraphNode, uint> traversalCost = null;
                    if (opts.accountForGridPenalties) {
                        traversalCost = (GraphNode node) => p.GetTraversalCost(node);
                    }
                    System.Func<GraphNode, bool> filter = (GraphNode node) => p.CanTraverse(node);
                    var result = GridStringPulling.Calculate(p.path, part.startIndex, part.endIndex, part.startPoint, part.endPoint, traversalCost, filter, int.MaxValue);
                    funnelPath.AddRange(result);
                    ListPool<Vector3>.Release(ref result);
                } else {
                    var portals = Funnel.ConstructFunnelPortals(p.path, part);
                    var result = Funnel.Calculate(portals, opts.splitAtEveryPortal);
                    funnelPath.AddRange(result);
                    ListPool<Vector3>.Release(ref portals.left);
                    ListPool<Vector3>.Release(ref portals.right);
                    ListPool<Vector3>.Release(ref result);
                }
            } else {
                // non-link parts will add the start/end points for the adjacent parts.
                // So if there is no non-link part before this one, then we need to add the start point of the link
                // and if there is no non-link part after this one, then we need to add the end point.
                if (i == 0 || parts[i - 1].type == Funnel.PartType.OffMeshLink) {
                    funnelPath.Add(part.startPoint);
                }
                if (i == parts.Count - 1 || parts[i + 1].type == Funnel.PartType.OffMeshLink) {
                    funnelPath.Add(part.endPoint);
                }
            }
        }

        UnityEngine.Assertions.Assert.IsTrue(funnelPath.Count >= 1);
        ListPool<Funnel.PathPart>.Release(ref parts);
        // Pool the previous vectorPath
        ListPool<Vector3>.Release(ref p.vectorPath);
        p.vectorPath = funnelPath;
    }

    public static void ApplyRadiusModifier(in RadiusModifierOptions opts, Path p, RadiusArrays arrays)
    {
        List<Vector3> vs = p.vectorPath;
        List<Vector3> res = MakeRadiusModifiedList(vs, opts.radius, opts.detail, arrays);

        if (res != vs) {
            ListPool<Vector3>.Release(ref p.vectorPath);
            p.vectorPath = res;
        }
    }

    public static void ApplyClampToGroundModifier(in ClampToGroundModifierOptions opts, Path path, in TerrainHeightData terrainMap)
    {
        List<Vector3> newList = ListPool<Vector3>.Claim();

        for (int i = 0; i < path.vectorPath.Count; i++)
        {
            Vector3 pos = path.vectorPath[i];
            float height = terrainMap.SampleHeight(pos);
            pos.y = height + opts.offset;
            newList.Add(pos);
        }

        ListPool<Vector3>.Release(ref path.vectorPath);
        path.vectorPath = newList;
    }

    //public static ClampPosToNavMesh(Path path, GraphNode prevNode, Vector3 prevPos)
    //{
    //    if (prevNode == null) {
    //        var nninfo = AstarPath.active.GetNearest(transform.position);
    //        prevNode = nninfo.node;
    //        prevPos = transform.position;
    //    }

    //    if (prevNode != null) {
    //        var graph = AstarData.GetGraph(prevNode) as IRaycastableGraph;
    //        if (graph != null) {
    //            GraphHitInfo hit;
    //            if (graph.Linecast(prevPos, transform.position, prevNode, out hit)) {
    //                hit.point.y = transform.position.y;
    //                Vector3 closest = VectorMath.ClosestPointOnLine(hit.tangentOrigin, hit.tangentOrigin + hit.tangent, transform.position);
    //                Vector3 ohit = hit.point;
    //                ohit = ohit + Vector3.ClampMagnitude((Vector3)hit.node.position - ohit, 0.008f);
    //                if (graph.Linecast(ohit, closest, hit.node, out hit)) {
    //                    hit.point.y = transform.position.y;
    //                    transform.position = hit.point;
    //                } else {
    //                    closest.y = transform.position.y;

    //                    transform.position = closest;
    //                }
    //            }
    //            prevNode = hit.node;
    //        }
    //    }

    //    prevPos = transform.position;
    //}

    public static void ApplyStartEndModifier(in StartEndModifierOptions opts, Path pth, ref List<GraphNode> connectionBuffer, ref System.Action<GraphNode> connectionBufferAddDelegate)
    {
        var p = pth as ABPath;

        // This modifier only supports ABPaths (doesn't make much sense for other paths anyway)
        if (p == null || p.vectorPath.Count == 0) { return; }

        bool singleNode = false;

        if (p.vectorPath.Count == 1 && !opts.addPoints) {
            // Duplicate first point
            p.vectorPath.Add(p.vectorPath[0]);
            singleNode = true;
        }

        // Add instead of replacing points
        bool forceAddStartPoint, forceAddEndPoint;
        // Which connection the start/end point was on (only used for the Connection mode)
        int closestStartConnection, closestEndConnection;

        Vector3 pStart = Snap(p, opts, opts.exactStartPoint, true, out forceAddStartPoint, out closestStartConnection, connectionBuffer, connectionBufferAddDelegate);
        Vector3 pEnd = Snap(p, opts, opts.exactEndPoint, false, out forceAddEndPoint, out closestEndConnection, connectionBuffer, connectionBufferAddDelegate);

        // This is a special case when the path is only a single node and the Connection mode is used.
        // (forceAddStartPoint/forceAddEndPoint is only used for the Connection mode)
        // In this case the start and end points lie on the connections of the node.
        // There are two cases:
        // 1. If the start and end points lie on the same connection we do *not* want
        // the path to pass through the node center but instead go directly from point to point.
        // This is the case of closestStartConnection == closestEndConnection.
        // 2. If the start and end points lie on different connections we *want*
        // the path to pass through the node center as it goes from one connection to another one.
        // However in any case we only want the node center to be added once to the path
        // so we set forceAddStartPoint to false anyway.
        if (singleNode) {
            if (closestStartConnection == closestEndConnection) {
                forceAddStartPoint = false;
                forceAddEndPoint = false;
            } else {
                forceAddStartPoint = false;
            }
        }

        // Add or replace the start point
        // Disable adding of points if the mode is SnapToNode since then
        // the first item in vectorPath will very likely be the same as the
        // position of the first node
        if ((forceAddStartPoint || opts.addPoints) && opts.exactStartPoint != Exactness.SnapToNode) {
            p.vectorPath.Insert(0, pStart);
        } else {
            p.vectorPath[0] = pStart;
        }

        if ((forceAddEndPoint || opts.addPoints) && opts.exactEndPoint != Exactness.SnapToNode) {
            p.vectorPath.Add(pEnd);
        } else {
            p.vectorPath[p.vectorPath.Count - 1] = pEnd;
        }
    }

    static Vector3 Snap(ABPath path, StartEndModifierOptions opts, Exactness mode, bool start, out bool forceAddPoint, out int closestConnectionIndex, List<GraphNode> connectionBuffer, System.Action<GraphNode> connectionBufferAddDelegate)
    {
        var index = start ? 0 : path.path.Count - 1;
        var node = path.path[index];
        var nodePos = (Vector3)node.position;

        closestConnectionIndex = 0;

        forceAddPoint = false;

        switch (mode) {
            case Exactness.ClosestOnNode: return start ? path.startPoint : path.endPoint;
            case Exactness.SnapToNode: return nodePos;
            case Exactness.Original:
            case Exactness.Interpolate:
            case Exactness.NodeConnection:
                Vector3 relevantPoint;
                if (start) {
                    //relevantPoint = adjustStartPoint != null ? adjustStartPoint() : path.originalStartPoint;
                    relevantPoint = path.originalStartPoint;
                } else {
                    relevantPoint = path.originalEndPoint;
                }

                switch (mode) {
                    case Exactness.Original:
                        return GetClampedPoint(nodePos, relevantPoint, node, opts.useRaycasting, opts.useGraphRaycasting, opts.mask);
                    case Exactness.Interpolate:
                        // Adjacent node to either the start node or the end node in the path
                        var adjacentNode = path.path[Mathf.Clamp(index + (start ? 1 : -1), 0, path.path.Count - 1)];
                        return VectorMath.ClosestPointOnSegment(nodePos, (Vector3)adjacentNode.position, relevantPoint);
                    case Exactness.NodeConnection:
                        // This code uses some tricks to avoid allocations
                        // even though it uses delegates heavily
                        // The connectionBufferAddDelegate delegate simply adds whatever node
                        // it is called with to the connectionBuffer
                        connectionBuffer = connectionBuffer ?? new List<GraphNode>();
                        connectionBufferAddDelegate = connectionBufferAddDelegate ?? (System.Action<GraphNode>)connectionBuffer.Add;

                        // Adjacent node to either the start node or the end node in the path
                        adjacentNode = path.path[Mathf.Clamp(index + (start ? 1 : -1), 0, path.path.Count - 1)];

                        // Add all neighbours of #node to the connectionBuffer
                        node.GetConnections(connectionBufferAddDelegate);
                        var bestPos = nodePos;
                        var bestDist = float.PositiveInfinity;

                        // Loop through all neighbours
                        // Do it in reverse order because the length of the connectionBuffer
                        // will change during iteration
                        for (int i = connectionBuffer.Count - 1; i >= 0; i--) {
                            var neighbour = connectionBuffer[i];

                            // Find the closest point on the connection between the nodes
                            // and check if the distance to that point is lower than the previous best
                            var closest = VectorMath.ClosestPointOnSegment(nodePos, (Vector3)neighbour.position, relevantPoint);

                            var dist = (closest - relevantPoint).sqrMagnitude;
                            if (dist < bestDist) {
                                bestPos = closest;
                                bestDist = dist;
                                closestConnectionIndex = i;

                                // If this node is not the adjacent node
                                // then the path should go through the start node as well
                                forceAddPoint = neighbour != adjacentNode;
                            }
                        }

                        connectionBuffer.Clear();
                        return bestPos;
                    default:
                        throw new System.ArgumentException("Cannot reach this point, but the compiler is not smart enough to realize that.");
                }
            default:
                throw new System.ArgumentException("Invalid mode");
        }
    }

    static Vector3 GetClampedPoint(Vector3 from, Vector3 to, GraphNode hint, bool useRaycasting, bool useGraphRaycasting, in LayerMask mask)
    {
        Vector3 point = to;
        RaycastHit hit;
        if (useRaycasting && Physics.Linecast(from, to, out hit, mask)) { point = hit.point; }

        if (useGraphRaycasting && hint != null) {
            var rayGraph = AstarData.GetGraph(hint) as IRaycastableGraph;
            if (rayGraph != null) {
                GraphHitInfo graphHit;
                if (rayGraph.Linecast(from, point, out graphHit)) {
                    point = graphHit.point;
                }
            }
        }
        return point;
    }

    //static Vector3 GetClampedPoint(in Vector3 from, in Vector3 to, GraphNode hint, bool useRaycasting, bool useGraphRaycasting, in LayerMask mask)
    //{
    //    Vector3 point = to;
    //    UnityEngine.RaycastHit hit;

    //    if (useRaycasting && Physics.Linecast(from, to, out hit, mask)) {
    //        point = hit.point;
    //    }

    //    if (useGraphRaycasting && hint != null) {
    //        IRaycastableGraph rayGraph = AstarData.GetGraph(hint) as IRaycastableGraph;

    //        if (rayGraph != null) {
    //            GraphHitInfo graphHit;
    //            if (rayGraph.Linecast(from, point, hint, out graphHit)) {
    //                point = graphHit.point;
    //            }
    //        }
    //    }

    //    return point;
    //}


    /// <summary>Apply this modifier on a raw Vector3 list</summary>
    static List<Vector3> MakeRadiusModifiedList(List<Vector3> vs, float radius, float detail, RadiusArrays arrays)
    {
        if (vs == null || vs.Count < 3) { return vs; }

        /// <summary>TODO: Do something about these allocations</summary>
        if (arrays.radi.Length < vs.Count) {
            arrays.radi = new float[vs.Count];
            arrays.a1 = new float[vs.Count];
            arrays.a2 = new float[vs.Count];
            arrays.dir = new bool[vs.Count];
        }

        for (int i = 0; i < vs.Count; i++) {
            arrays.radi[i] = radius;
        }

        arrays.radi[0] = 0;
        arrays.radi[vs.Count - 1] = 0;

        int count = 0;
        for (int i = 0; i < vs.Count - 1; i++) {
            count++;
            if (count > 2 * vs.Count) {
                Debug.LogWarning("Could not resolve radiuses, the path is too complex. Try reducing the base radius");
                break;
            }

            TangentType tt;

            if (i == 0) {
                tt = CalculateTangentTypeSimple(vs[i], vs[i + 1], vs[i + 2]);
            } else if (i == vs.Count - 2) {
                tt = CalculateTangentTypeSimple(vs[i - 1], vs[i], vs[i + 1]);
            } else {
                tt = CalculateTangentType(vs[i - 1], vs[i], vs[i + 1], vs[i + 2]);
            }

            //DrawCircle (vs[i], radi[i], Color.yellow);

            if ((tt & TangentType.Inner) != 0) {
                //Angle to tangent
                float a;
                //Angle to other circle
                float sigma;

                //Calculate angles to the next circle and angles for the inner tangents
                if (!CalculateCircleInner(vs[i], vs[i + 1], arrays.radi[i], arrays.radi[i + 1], out a, out sigma)) {
                    //Failed, try modifying radiuses
                    float magn = (vs[i + 1] - vs[i]).magnitude;
                    arrays.radi[i] = magn * (arrays.radi[i] / (arrays.radi[i] + arrays.radi[i + 1]));
                    arrays.radi[i + 1] = magn - arrays.radi[i];
                    arrays.radi[i] *= 0.99f;
                    arrays.radi[i + 1] *= 0.99f;
                    i -= 2;
                    continue;
                }

                if (tt == TangentType.InnerRightLeft) {
                    arrays.a2[i] = sigma - a;
                    arrays.a1[i + 1] = sigma - a + (float)System.Math.PI;
                    arrays.dir[i] = true;
                } else {
                    arrays.a2[i] = sigma + a;
                    arrays.a1[i + 1] = sigma + a + (float)System.Math.PI;
                    arrays.dir[i] = false;
                }
            } else {
                float sigma;
                float a;

                //Calculate angles to the next circle and angles for the outer tangents
                if (!CalculateCircleOuter(vs[i], vs[i + 1], arrays.radi[i], arrays.radi[i + 1], out a, out sigma)) {
                    //Failed, try modifying radiuses
                    if (i == vs.Count - 2) {
                        //The last circle has a fixed radius at 0, don't modify it
                        arrays.radi[i] = (vs[i + 1] - vs[i]).magnitude;
                        arrays.radi[i] *= 0.99f;
                        i -= 1;
                    } else {
                        if (arrays.radi[i] > arrays.radi[i + 1]) {
                            arrays.radi[i + 1] = arrays.radi[i] - (vs[i + 1] - vs[i]).magnitude;
                        } else {
                            arrays.radi[i + 1] = arrays.radi[i] + (vs[i + 1] - vs[i]).magnitude;
                        }
                        arrays.radi[i + 1] *= 0.99f;
                    }

                    i -= 1;
                    continue;
                }

                if (tt == TangentType.OuterRight) {
                    arrays.a2[i] = sigma - a;
                    arrays.a1[i + 1] = sigma - a;
                    arrays.dir[i] = true;
                } else {
                    arrays.a2[i] = sigma + a;
                    arrays.a1[i + 1] = sigma + a;
                    arrays.dir[i] = false;
                }
            }
        }

        List<Vector3> res = ListPool<Vector3>.Claim();
        res.Add(vs[0]);
        if (detail < 1)
            detail = 1;
        float step = (float)(2 * System.Math.PI) / detail;
        for (int i = 1; i < vs.Count - 1; i++) {
            float start = arrays.a1[i];
            float end = arrays.a2[i];
            float rad = arrays.radi[i];

            if (arrays.dir[i]) {
                if (end < start)
                    end += (float)System.Math.PI * 2;
                for (float t = start; t < end; t += step) {
                    res.Add(new Vector3((float)System.Math.Cos(t), 0, (float)System.Math.Sin(t)) * rad + vs[i]);
                }
            } else {
                if (start < end)
                    start += (float)System.Math.PI * 2;
                for (float t = start; t > end; t -= step) {
                    res.Add(new Vector3((float)System.Math.Cos(t), 0, (float)System.Math.Sin(t)) * rad + vs[i]);
                }
            }
        }

        res.Add(vs[vs.Count - 1]);

        return res;
    }

    static List<Vector3> CurvedNonuniform(List<Vector3> path, float maxSegmentLength, float factor)
    {
        if (maxSegmentLength <= 0) {
            Debug.LogWarning("Max Segment Length is <= 0 which would cause DivByZero-exception or other nasty errors (avoid this)");
            return path;
        }

        int pointCounter = 0;
        for (int i = 0; i < path.Count - 1; i++) {
            //pointCounter += Mathf.FloorToInt ((path[i]-path[i+1]).magnitude / maxSegmentLength)+1;

            float dist = (path[i] - path[i + 1]).magnitude;
            //In order to avoid floating point errors as much as possible, and in lack of a better solution
            //loop through it EXACTLY as the other code further down will
            for (float t = 0; t <= dist; t += maxSegmentLength) {
                pointCounter++;
            }
        }

        List<Vector3> subdivided = ListPool<Vector3>.Claim(pointCounter);

        // Set first velocity
        Vector3 preEndVel = (path[1] - path[0]).normalized;

        for (int i = 0; i < path.Count - 1; i++) {
            float dist = (path[i] - path[i + 1]).magnitude;

            Vector3 startVel1 = preEndVel;
            Vector3 endVel1 = i < path.Count - 2 ? ((path[i + 2] - path[i + 1]).normalized - (path[i] - path[i + 1]).normalized).normalized : (path[i + 1] - path[i]).normalized;

            Vector3 startVel = dist * factor * startVel1;
            Vector3 endVel = dist * factor * endVel1;

            Vector3 start = path[i];
            Vector3 end = path[i + 1];

            float onedivdist = 1F / dist;

            for (float t = 0; t <= dist; t += maxSegmentLength) {
                float t2 = t * onedivdist;

                subdivided.Add(GetPointOnCubic(start, end, startVel, endVel, t2));
            }

            preEndVel = endVel1;
        }

        subdivided[subdivided.Count - 1] = path[path.Count - 1];

        return subdivided;
    }

    static List<Vector3> SmoothOffsetSimple(List<Vector3> path, int iterations, float offset)
    {
        if (path.Count <= 2 || iterations <= 0) {
            return path;
        }

        if (iterations > 12) {
            Debug.LogWarning("A very high iteration count was passed, won't let this one through");
            return path;
        }

        int maxLength = (path.Count - 2) * (int)Mathf.Pow(2, iterations) + 2;

        List<Vector3> subdivided = ListPool<Vector3>.Claim(maxLength);
        List<Vector3> subdivided2 = ListPool<Vector3>.Claim(maxLength);

        for (int i = 0; i < maxLength; i++) { subdivided.Add(Vector3.zero); subdivided2.Add(Vector3.zero); }

        for (int i = 0; i < path.Count; i++) {
            subdivided[i] = path[i];
        }

        for (int iteration = 0; iteration < iterations; iteration++) {
            int currentPathLength = (path.Count - 2) * (int)Mathf.Pow(2, iteration) + 2;

            //Switch the arrays
            List<Vector3> tmp = subdivided;
            subdivided = subdivided2;
            subdivided2 = tmp;

            const float nextMultiplier = 1F;

            for (int i = 0; i < currentPathLength - 1; i++) {
                Vector3 current = subdivided2[i];
                Vector3 next = subdivided2[i + 1];

                Vector3 normal = Vector3.Cross(next - current, Vector3.up);
                normal = normal.normalized;

                bool firstRight = false;
                bool secondRight = false;
                bool setFirst = false;
                bool setSecond = false;
                if (i != 0 && !VectorMath.IsColinearXZ(current, next, subdivided2[i - 1])) {
                    setFirst = true;
                    firstRight = VectorMath.RightOrColinearXZ(current, next, subdivided2[i - 1]);
                }
                if (i < currentPathLength - 1 && !VectorMath.IsColinearXZ(current, next, subdivided2[i + 2])) {
                    setSecond = true;
                    secondRight = VectorMath.RightOrColinearXZ(current, next, subdivided2[i + 2]);
                }

                if (setFirst) {
                    subdivided[i * 2] = current + (firstRight ? offset * nextMultiplier * normal : offset * nextMultiplier * -normal);
                } else {
                    subdivided[i * 2] = current;
                }

                if (setSecond) {
                    subdivided[i * 2 + 1] = next + (secondRight ? offset * nextMultiplier * normal : offset * nextMultiplier * -normal);
                } else {
                    subdivided[i * 2 + 1] = next;
                }
            }

            subdivided[(path.Count - 2) * (int)Mathf.Pow(2, iteration + 1) + 2 - 1] = subdivided2[currentPathLength - 1];
        }

        ListPool<Vector3>.Release(ref subdivided2);

        return subdivided;
    }

    static List<Vector3> SmoothSimple(List<Vector3> path, bool uniformLength, float maxSegmentLength, int subdivisions, float strength, int iterations)
    {
        if (path.Count < 2)
            return path;

        List<Vector3> subdivided;

        if (uniformLength) {
            // Clamp to a small value to avoid the path being divided into a huge number of segments
            maxSegmentLength = Mathf.Max(maxSegmentLength, 0.005f);

            float pathLength = 0;
            for (int i = 0; i < path.Count - 1; i++) {
                pathLength += Vector3.Distance(path[i], path[i + 1]);
            }

            int estimatedNumberOfSegments = Mathf.FloorToInt(pathLength / maxSegmentLength);
            // Get a list with an initial capacity high enough so that we can add all points
            subdivided = ListPool<Vector3>.Claim(estimatedNumberOfSegments + 2);

            float distanceAlong = 0;

            // Sample points every [maxSegmentLength] world units along the path
            for (int i = 0; i < path.Count - 1; i++) {
                var start = path[i];
                var end = path[i + 1];

                float length = Vector3.Distance(start, end);

                while (distanceAlong < length) {
                    subdivided.Add(Vector3.Lerp(start, end, distanceAlong / length));
                    distanceAlong += maxSegmentLength;
                }

                distanceAlong -= length;
            }

            // Make sure we get the exact position of the last point
            subdivided.Add(path[path.Count - 1]);
        } else {
            subdivisions = Mathf.Max(subdivisions, 0);

            if (subdivisions > 10) {
                Debug.LogWarning("Very large number of subdivisions. Cowardly refusing to subdivide every segment into more than " + (1 << subdivisions) + " subsegments");
                subdivisions = 10;
            }

            int steps = 1 << subdivisions;
            subdivided = ListPool<Vector3>.Claim((path.Count - 1) * steps + 1);
            Polygon.Subdivide(path, subdivided, steps);
        }

        if (strength > 0) {
            for (int it = 0; it < iterations; it++) {
                Vector3 prev = subdivided[0];

                for (int i = 1; i < subdivided.Count - 1; i++) {
                    Vector3 tmp = subdivided[i];

                    // prev is at this point set to the value that subdivided[i-1] had before this loop started
                    // Move the point closer to the average of the adjacent points
                    subdivided[i] = Vector3.Lerp(tmp, (prev + subdivided[i + 1]) / 2F, strength);

                    prev = tmp;
                }
            }
        }

        return subdivided;
    }

    static List<Vector3> SmoothBezier(List<Vector3> path, int subdivisions, float bezierTangentLength)
    {
        if (subdivisions < 0)
            subdivisions = 0;

        int subMult = 1 << subdivisions;
        List<Vector3> subdivided = ListPool<Vector3>.Claim();

        for (int i = 0; i < path.Count - 1; i++) {
            Vector3 tangent1;
            Vector3 tangent2;
            if (i == 0) {
                tangent1 = path[i + 1] - path[i];
            } else {
                tangent1 = path[i + 1] - path[i - 1];
            }

            if (i == path.Count - 2) {
                tangent2 = path[i] - path[i + 1];
            } else {
                tangent2 = path[i] - path[i + 2];
            }

            tangent1 *= bezierTangentLength;
            tangent2 *= bezierTangentLength;

            Vector3 v1 = path[i];
            Vector3 v2 = v1 + tangent1;
            Vector3 v4 = path[i + 1];
            Vector3 v3 = v4 + tangent2;

            for (int j = 0; j < subMult; j++) {
                subdivided.Add(AstarSplines.CubicBezier(v1, v2, v3, v4, (float)j / subMult));
            }
        }

        // Assign the last point
        subdivided.Add(path[path.Count - 1]);

        return subdivided;
    }

    static Vector3 GetPointOnCubic(Vector3 a, Vector3 b, Vector3 tan1, Vector3 tan2, float t)
    {
        float t2 = t * t, t3 = t2 * t;

        float h1 = 2 * t3 - 3 * t2 + 1;          // calculate basis function 1
        float h2 = -2 * t3 + 3 * t2;              // calculate basis function 2
        float h3 = t3 - 2 * t2 + t;          // calculate basis function 3
        float h4 = t3 - t2;                // calculate basis function 4

        // multiply and sum all funtions together to build the interpolated point along the curve.
        return h1 * a + h2 * b + h3 * tan1 + h4 * tan2;
    }

    static bool CalculateCircleInner(Vector3 p1, Vector3 p2, float r1, float r2, out float a, out float sigma)
    {
        float dist = (p1 - p2).magnitude;

        if (r1 + r2 > dist) {
            a = 0;
            sigma = 0;
            return false;
        }

        a = (float)System.Math.Acos((r1 + r2) / dist);

        sigma = (float)System.Math.Atan2(p2.z - p1.z, p2.x - p1.x);
        return true;
    }

    static bool CalculateCircleOuter(Vector3 p1, Vector3 p2, float r1, float r2, out float a, out float sigma)
    {
        float dist = (p1 - p2).magnitude;

        if (System.Math.Abs(r1 - r2) > dist) {
            a = 0;
            sigma = 0;
            return false;
        }
        a = (float)System.Math.Acos((r1 - r2) / dist);
        sigma = (float)System.Math.Atan2(p2.z - p1.z, p2.x - p1.x);
        return true;
    }

    enum TangentType
    {
        OuterRight = 1 << 0,
        InnerRightLeft = 1 << 1,
        InnerLeftRight = 1 << 2,
        OuterLeft = 1 << 3,
        Outer = OuterRight | OuterLeft,
        Inner = InnerRightLeft | InnerLeftRight
    }

    static TangentType CalculateTangentType(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        bool l1 = VectorMath.RightOrColinearXZ(p1, p2, p3);
        bool l2 = VectorMath.RightOrColinearXZ(p2, p3, p4);

        return (TangentType)(1 << ((l1 ? 2 : 0) + (l2 ? 1 : 0)));
    }

    static TangentType CalculateTangentTypeSimple(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        bool l2 = VectorMath.RightOrColinearXZ(p1, p2, p3);
        bool l1 = l2;

        return (TangentType)(1 << ((l1 ? 2 : 0) + (l2 ? 1 : 0)));
    }
}