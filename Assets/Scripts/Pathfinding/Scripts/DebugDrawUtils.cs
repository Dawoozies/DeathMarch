// This utility class provides some extra debug draw functions
//
// a couple of the functions are from the Latios Framework (DreamingImLatios)
//--------------------------------------------------------------------------------------------------//

using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using System.Linq;

public enum MapXYToType { XZ, ZY, XY }
public static class DebugDrawUtils
{
    // Draw a line from start to end.
    // start --> start point
    // end --> stop point
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // color -> the color to draw the line
    public static void DrawLine(in float3 start, in float3 end, in Color color, float duration)
    {
        Debug.DrawLine(start, end, color, duration);
    }

    // Draw a line from start to start + dir in world coordinates
    // start --> start point
    // dir --> direction and length of the ray
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // color -> the color to draw the line
    public static void DrawRay(in float3 start, in float3 dir, in Color color, float duration)
    {
        Debug.DrawRay(start, dir, color, duration);
    }

    // Draw the specified rectangle at the specified height
    // rect --> the rectangle to draw (world coords)
    // h --> height of the rectangle's offset
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawGroundRect(in Rect rect, float h, in Color color, float duration)
    {
        Debug.DrawLine(new Vector3(rect.x, h, rect.y), new Vector3(rect.x + rect.width, h, rect.y), color, duration);
        Debug.DrawLine(new Vector3(rect.x, h, rect.y), new Vector3(rect.x, h, rect.y + rect.height), color, duration);
        Debug.DrawLine(new Vector3(rect.x + rect.width, h, rect.y + rect.height), new Vector3(rect.x + rect.width, h, rect.y), color, duration);
        Debug.DrawLine(new Vector3(rect.x + rect.width, h, rect.y + rect.height), new Vector3(rect.x, h, rect.y + rect.height), color, duration);
    }

    // Draw a 3D crosshair (reticle)
    // origin --> the centerpoint of the cross
    // size --> the amount to extend past the origin for each axis (radius)
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawCross(Vector3 origin, float size, Color color, float duration)
    {
        Vector3 line1Start = origin + (Vector3.right * size);
        Vector3 line1End = origin - (Vector3.right * size);
        Debug.DrawLine(line1Start, line1End, color, duration);
        Vector3 line2Start = origin + (Vector3.up * size);
        Vector3 line2End = origin - (Vector3.up * size);
        Debug.DrawLine(line2Start, line2End, color, duration);
        Vector3 line3Start = origin + (Vector3.forward * size);
        Vector3 line3End = origin - (Vector3.forward * size);
        Debug.DrawLine(line3Start, line3End, color, duration);
    }

    // draw a wire box matching to the specified bounds (without rotation)
    // bounds --> specifies the box to draw
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireBox(in Bounds bounds, in Color color, float duration)
    {
        float3 frontTopLeft = new float3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);  // Front top left corner
        float3 frontTopRight = new float3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);  // Front top right corner
        float3 frontBottomLeft = new float3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);  // Front bottom left corner
        float3 frontBottomRight = new float3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);  // Front bottom right corner
        float3 backTopLeft = new float3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);  // Back top left corner
        float3 backTopRight = new float3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);  // Back top right corner
        float3 backBottomLeft = new float3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);  // Back bottom left corner
        float3 backBottomRight = new float3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);  // Back bottom right corner

        Debug.DrawLine(frontTopLeft, frontTopRight, color, duration);
        Debug.DrawLine(frontTopRight, frontBottomRight, color, duration);
        Debug.DrawLine(frontBottomRight, frontBottomLeft, color, duration);
        Debug.DrawLine(frontBottomLeft, frontTopLeft, color, duration);

        Debug.DrawLine(backTopLeft, backTopRight, color, duration);
        Debug.DrawLine(backTopRight, backBottomRight, color, duration);
        Debug.DrawLine(backBottomRight, backBottomLeft, color, duration);
        Debug.DrawLine(backBottomLeft, backTopLeft, color, duration);

        Debug.DrawLine(frontTopLeft, backTopLeft, color, duration);
        Debug.DrawLine(frontTopRight, backTopRight, color, duration);
        Debug.DrawLine(frontBottomRight, backBottomRight, color, duration);
        Debug.DrawLine(frontBottomLeft, backBottomLeft, color, duration);
    }

    //// Draws a unity physics AABB wire box (without rotation)
    //// box --> the aabb box
    //// color --> color to use
    //// duration --> use zero for just one frame, otherwise it is a time in seconds
    //public static void DrawWireBox(in Unity.Physics.Aabb box, in Color color, float duration)
    //{
    //    DrawWireBox(new Bounds(box.Center, box.Extents), color, duration);
    //}

    // draws a wire box with a given centerpoint, rotation, and size
    // center --> the origin of the box
    // rotation --> a quaternion representing the rotation of the box
    // size --> the amount to extend past the origin for each axis (radius)
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireBox(float3 center, quaternion rotation, float3 size, Color color, float duration)
    {
        if (color.Equals(default)) color = Color.white;

        var matrix = float4x4.TRS(center, rotation, 1);
        //front corners
        var forwardRightUpper = math.transform(matrix, new float3(size.x / 2, size.y / 2, size.z / 2));
        var forwardRightLower = math.transform(matrix, new float3(size.x / 2, -size.y / 2, size.z / 2));
        var forwardLeftUpper = math.transform(matrix, new float3(-size.x / 2, size.y / 2, size.z / 2));
        var forwardLeftLower = math.transform(matrix, new float3(-size.x / 2, -size.y / 2, size.z / 2));
        //back corners
        var backRightUpper = math.transform(matrix, new float3(size.x / 2, size.y / 2, -size.z / 2));
        var backRightLower = math.transform(matrix, new float3(size.x / 2, -size.y / 2, -size.z / 2));
        var backLeftUpper = math.transform(matrix, new float3(-size.x / 2, size.y / 2, -size.z / 2));
        var backLeftLower = math.transform(matrix, new float3(-size.x / 2, -size.y / 2, -size.z / 2));
        //square front
        Debug.DrawLine(forwardRightUpper, forwardRightLower, color, duration);
        Debug.DrawLine(forwardRightUpper, forwardLeftUpper, color, duration);
        Debug.DrawLine(forwardRightLower, forwardLeftLower, color, duration);
        Debug.DrawLine(forwardLeftLower, forwardLeftUpper, color, duration);
        //square back
        Debug.DrawLine(backRightUpper, backRightLower, color, duration);
        Debug.DrawLine(backRightUpper, backLeftUpper, color, duration);
        Debug.DrawLine(backRightLower, backLeftLower, color, duration);
        Debug.DrawLine(backLeftLower, backLeftUpper, color, duration);
        //attach squares
        Debug.DrawLine(backRightUpper, forwardRightUpper, color, duration);
        Debug.DrawLine(backLeftUpper, forwardLeftUpper, color, duration);
        Debug.DrawLine(backRightLower, forwardRightLower, color, duration);
        Debug.DrawLine(backLeftLower, forwardLeftLower, color, duration);
    }

    // draw a square wire box centered at pos
    // pos --> the origin/center of the box
    // extent --> the amount to extend the box in each size
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireBox(in float3 pos, float extent, in Color color, float duration)
    {
        DrawWireBox(new Bounds { center = pos, extents = new Vector3(extent, extent, extent) }, color, duration);
    }

    // draw a wire box centered at pos
    // pos --> the origin/center of the box
    // extents --> the amount to extend the box in each axis
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // ** if you want to draw a Unity.Physics.Aabb you can use
    //    DrawWireBox(aabb.Center, aabb.Extents, color, duration);
    public static void DrawWireBox(in float3 pos, float3 extents, in Color color, float duration)
    {
        DrawWireBox(new Bounds(pos, extents), color, duration);
    }

    // draw a circle on the ground at the specified position (assuming +z is up)
    // pos --> coordinates to draw at (assuming +z is up)
    // radius --> the radius of the circle
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // thetaScale --> the quality (affects number of points around the circle)
    public static void DrawGroundCircle(float3 pos, float radius, Color color, float duration, float thetaScale = 0.1f)
    {
        DrawEllipse(pos, math.up(), math.forward(), radius, radius, color, duration, thetaScale);
    }

    // draw a circle on the ground at the specified position (assuming +z is up)
    // pos --> coordinates to draw at (assuming +z is up)
    // radiusX --> the x-axis radius of the ellipse
    // radiusY --> the y-axis radius of the ellipse
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // thetaScale --> the quality (affects number of points around the circle)
    public static void DrawGroundEllipse(float3 pos, float radiusX, float radiusY, Color color, float duration, float thetaScale = 0.1f)
    {
        DrawEllipse(pos, math.up(), math.forward(), radiusX, radiusY, color, duration, thetaScale);
    }

    // draw a circle given the specified up and forward directions
    // pos --> coordinates to draw at (assuming +z is up)
    // forward --> the forward direction of the circle
    // up --> the up vector perpendicular to the circle
    // radius --> the radius of the circle
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // thetaScale --> the quality (affects number of points around the circle)
    public static void DrawCircle(float3 pos, float3 forward, float3 up, float radius, Color color, float duration, float thetaScale = 0.1f)
    {
        DrawEllipse(pos, forward, up, radius, radius, color, duration, thetaScale);
    }

    // draws an ellipse given the specified up and forward directions
    // pos --> coordinates to draw at (assuming +z is up)
    // forward --> the forward direction of the ellipse
    // up --> the up vector perpendicular to the ellipse
    // radiusX --> the x-axis radius of the ellipse
    // radiusY --> the y-axis radius of the ellipse
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // thetaScale --> the quality (affects number of points around the circle)
    public static void DrawEllipse(float3 pos, float3 forward, float3 up, float radiusX, float radiusY, Color color, float duration, float thetaScale = 0.1f)
    {
        float angle = 0f;
        quaternion rot = quaternion.LookRotation(forward, up);
        float3 lastPoint = float3.zero;
        float3 thisPoint = float3.zero;
        int pointCount = (int)((1f / thetaScale) + 1f);

        for (int i = 0; i < pointCount + 1; i++) {
            thisPoint.x = math.sin(angle) * radiusX;
            thisPoint.y = math.cos(angle) * radiusY;
            if (i > 0) {
                Debug.DrawLine(math.mul(rot, lastPoint) + pos, math.mul(rot, thisPoint) + pos, color, duration);
            }
            lastPoint = thisPoint;
            angle += 6.28318531f / pointCount; // 6.28318531 is 360 degrees in radians
        }
    }

    // draw a wire capsule at the specified position
    // center --> coordinates to draw at (assuming +z is up)
    // rotation --> the rotation quaternion of the capsule
    // radius --> the radius of the capsule
    // height --> the height of the capsule 
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireCapsule(float3 center, quaternion rotation, float radius, float height, Color color, float duration)
    {
        if (color.Equals(default)) color = Color.white;

        var matrix = float4x4.TRS(center, rotation, 1);
        var originPoint = math.transform(matrix, new float3(0, 0, height / 2 - radius));
        var destinationPoint = math.transform(matrix, -new float3(0, 0, height / 2 - radius));
        //set new rotation offset for domes
        var rot = math.mul(rotation, quaternion.Euler(math.radians(90), 0, 0));
        //draw upper dome
        DrawWireArc(originPoint, rot, radius, 180, 0, MapXYToType.XY, 18, color: color, duration);
        DrawWireArc(originPoint, rot, radius, 180, 0, mapXYTo: MapXYToType.ZY, 18, color, duration);
        DrawWireCircle(originPoint, rot, radius, mapXYTo: MapXYToType.XZ, color, duration);
        //draw lower dome
        DrawWireArc(destinationPoint, rot, radius, 360, 180, MapXYToType.XY, 18, color, duration);
        DrawWireArc(destinationPoint, rot, radius, 360, 180, mapXYTo: MapXYToType.ZY, 18, color, duration);
        DrawWireCircle(destinationPoint, rot, radius, mapXYTo: MapXYToType.XZ, color, duration);
        //connect domes
        matrix = float4x4.TRS(originPoint, rotation, 1);
        var farFront = math.transform(matrix, math.up() * radius);
        var farBack = math.transform(matrix, math.down() * radius);
        var farLeft = math.transform(matrix, math.left() * radius);
        var farRight = math.transform(matrix, math.right() * radius);
        matrix = float4x4.TRS(destinationPoint, rotation, 1);
        var closeFront = math.transform(matrix, math.up() * radius);
        var closeBack = math.transform(matrix, math.down() * radius);
        var closeLeft = math.transform(matrix, math.left() * radius);
        var closeRight = math.transform(matrix, math.right() * radius);
        Debug.DrawLine(farFront, closeFront, color, duration);
        Debug.DrawLine(farBack, closeBack, color, duration);
        Debug.DrawLine(farLeft, closeLeft, color, duration);
        Debug.DrawLine(farRight, closeRight, color, duration);
    }

    // draw a sphere-cast at the specified position
    // origin --> coordinates to draw at (assuming +z is up)
    // direction --> the cast direction
    // radius --> the radius of the sphere
    // maxDistance --> the max distance added to the height
    // color --> color to use
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawSphereCast(float3 origin, float3 direction, float radius, float maxDistance, Color color, float duration)
    {
        if (color.Equals(default)) color = Color.white;
        var rot = direction.Equals(math.down()) ? quaternion.Euler(math.radians(90), 0, 0) : direction.Equals(math.up()) ? quaternion.Euler(math.radians(-90), 0, 0) : quaternion.LookRotation(direction, math.up());
        float height = (radius * 2) + maxDistance;
        DrawWireCapsule(origin + (direction * ((height / 2) - radius)), rot, radius, height, color, duration);
    }

    // draw a wire sphere at the specified position
    // center --> coordinates to draw at (assuming +z is up)
    // rotation --> the rotation quaternion for the sphere
    // radius --> the radius of the sphere
    // color --> color to draw with
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireSphere(float3 center, quaternion rotation, float radius, Color color, float duration)
    {
        if (color.Equals(default)) color = Color.white;
        DrawWireCircle(center, rotation, radius, MapXYToType.XY, color, duration);
        DrawWireCircle(center, rotation, radius, MapXYToType.XZ, color, duration);
        DrawWireCircle(center, rotation, radius, MapXYToType.ZY, color, duration);
    }

    // draw a wire circle at the specified point
    // center --> coordinates to draw at (assuming +z is up)
    // rotation --> the rotation quaternion for the circle
    // radius --> the radius of the circle
    // mapXYTo --> plane the circle is mapped to (default MapXYToType.XY)
    // color --> color to draw with
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireCircle(float3 center, quaternion rotation, float radius, MapXYToType mapXYTo = MapXYToType.XY, Color color = default, float duration = 0f)
    {
        if (color.Equals(default)) color = Color.white;
        DrawWireArc(center, rotation, radius, 360, 0, mapXYTo: mapXYTo, 18, color, duration);
    }

    // draw a wire arc at the specified point
    // center --> coordinates to draw at (assuming +z is up)
    // rotation --> the rotation quaternion for the arc
    // radius --> the radius of the arc
    // angle --> the arc angle
    // startAngle --> the starting arc angle
    // mapXYTo --> plane the arc is mapped to (default MapXYToType.XY)
    // color --> color to draw with
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    public static void DrawWireArc(float3 center, quaternion rotation, float radius, float angle, float startAngle, MapXYToType mapXYTo = MapXYToType.XY, float quality = 18, Color color = default, float duration = 0f)
    {
        if (color.Equals(default)) color = Color.white;
        var matrix = float4x4.TRS(center, rotation, 1);

        float step = startAngle;
        float inc = math.distance(angle, startAngle) / quality;
        for (int i = 0; step < angle; i++) {
            var startPoint = GetCirclePoint(matrix, radius, step, mapXYTo);

            step += inc;
            var endPoint = GetCirclePoint(matrix, radius, step, mapXYTo);

            Debug.DrawLine(startPoint, endPoint, color, duration);
        }
    }

    // draw a path that is made out of DynamicBuffer<WaypointData>
    // path --> a list of points representing the path
    // color --> color to draw with
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // startIndex --> the index of the starting point
    // endIndex --> the index of the ending point (if set to -1, all points after startIndex will be drawn)
    public static void DrawPath(DynamicBuffer<WaypointData> path, Color color, float duration, int startIndex = 0, int endIndex = -1)
    {
        endIndex = endIndex < 0 ? path.Length : endIndex;
        for (int i = startIndex; i < endIndex; i++) {
            if (i > 0) { DrawLine(path[i - 1].Point, path[i].Point, color, duration); }
            DrawCross(path[i].Point, 0.3f, color, duration);
        }
    }

    // draw a path that is made out of List<Vector3>
    // path --> a list of points representing the path
    // color --> color to draw with
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // startIndex --> the index of the starting point
    // endIndex --> the index of the ending point (if set to -1, all points after startIndex will be drawn)
    public static void DrawPath(List<Vector3> path, Color color, float duration = 100f, int startIndex = 0, int endIndex = -1)
    {
        endIndex = endIndex < 0 ? path.Count : endIndex;
        for (int i = startIndex; i < endIndex; i++) {
            if (i > 0) { DrawLine(path[i - 1], path[i], color, duration); }
        }
    }

    // draw all the points for each path (where each point is represented by a 3D cross)
    // path --> a list of points representing the path
    // color --> color to draw with
    // duration --> use zero for just one frame, otherwise it is a time in seconds
    // startIndex --> the index of the starting point
    // endIndex --> the index of the ending point (if set to -1, all points after startIndex will be drawn)
    public static void DrawPathWaypoints(List<Vector3> path, Color color, float duration = 100f, int startIndex = 0, int endIndex = -1)
    {
        if (endIndex < 0) { endIndex = path.Count; }
        endIndex++; // turn index into length
        for (int i = startIndex; i < endIndex; i++) {
            DrawCross(path[i], 0.3f, color, duration);
        }
    }

    // TODO - UNTESTED FUNCTION
    public static void SpawnSphereEntity(EntityManager em, float3 pos, float radius)
    {
        EntityArchetype arch = em.CreateArchetype(typeof(LocalTransform), typeof(LocalToWorld), typeof(RenderMeshUnmanaged));
        Entity entity = em.CreateEntity(arch);
        em.SetComponentData(entity, new LocalTransform { Position = pos, Rotation = quaternion.identity, Scale = radius });
        //em.SetComponentData(entity, new Translation { Value = pos });
        //em.SetComponentData(entity, new Scale { Value = radius });
    }

    // spawn a sphere game-object at the specified position
    // pos --> coordinate for the sphere (world coords)
    // radius --> radius of the sphere (1 is unit sphere)
    // name --> name to give to the spawned game-object
    public static void SpawnSphereGameObject(float3 pos, float radius, string name)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(radius, radius, radius);
        go.name = name;
    }

    // spawn a cube game-object
    // bounds --> defines the cube location and size
    // name --> name to give to the spawned game-object
    public static void SpawnCubeGameObject(Bounds bounds, string name)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = bounds.center;
        go.transform.localScale = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        go.name = name;
    }


    //############################## PROTECTED ##############################//

    private static float3 GetCirclePoint(float4x4 matrix, float radius, float step, MapXYToType mapXYTo)
    {
        var x = radius * math.cos(math.radians(step));
        var y = radius * math.sin(math.radians(step));
        return mapXYTo == MapXYToType.XZ ? math.transform(matrix, new float3(x, 0, y)) : mapXYTo == MapXYToType.ZY ? math.transform(matrix, new float3(0, y, x)) : math.transform(matrix, new float3(x, y, 0));
    }
}