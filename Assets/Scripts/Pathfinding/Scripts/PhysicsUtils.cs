// This is a helper class that contains useful physics functions
// If the terrain changes, you should call ReInitialize()
// Currently this class only works with one terrain at a time.
// It uses PhysicsMaskType, so you must also have PhysicsMaskType.cs in your project.
// It also uses TerrainTag - you must have TerrainTag placed on your terrain
//
// TerrainStab() will shoot a ray from the camera to the terrain (and only the terrain) so it is relatively fast.
//   If you are in a system, the best way to fetch the terrain collider is:
//      PhysicsCollider terrainCollider = PhysicsUtils.GetPhysicsCollider(GetSingletonEntity<TerrainTag>());
//   If you are outside of a system, you can use FindTerrainColliderFromDefaultWorld()
// Pick() will shoot a ray from the camera into the world and return the first object that it hits
//
// WARNING - DO NOT EVER ADD using UnityEngine HERE! It will conflict with Unity.Physics.
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections;

public static class PhysicsUtils
{
    //public static Entity FindTerrainEntityInDefaultWorld()
    //{
    //    return World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<TerrainTag>()).GetSingletonEntity();
    //}

    //public static BuildPhysicsWorld GetDefaultBuildPhysicsWorld()
    //{
    //    //return World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
    //    return World.GetOrCreateSystem<BuildPhysicsWorld>();
    //}

    // fetch the default collision world - takes 4 lines of code because unity has gone insane.
    public static CollisionWorld GetDefaultCollisionWorld()
    {     
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();
        builder.Dispose();
        return collisionWorld;
    }

    public static PhysicsWorldSingleton QueryPhysicsSingleton()
    {     
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        PhysicsWorldSingleton physSingleton = singletonQuery.GetSingleton<PhysicsWorldSingleton>();
        singletonQuery.Dispose();
        builder.Dispose();
        return physSingleton;
    }

    //public static PhysicsCollider FindTerrainColliderFromDefaultWorld()
    //{
    //    return GetPhysicsCollider(FindTerrainEntityInDefaultWorld(), World.DefaultGameObjectInjectionWorld.EntityManager);
    //}

    public static PhysicsCollider GetPhysicsCollider(Entity entity, EntityManager em)
    {
        return em.GetComponentData<PhysicsCollider>(entity);
    }

    public static PhysicsCollider GetPhysicsColliderFromDefaultWorld(Entity entity)
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<PhysicsCollider>(entity);
    }

    //public static CollisionFilter MakeEverythingFilter(PhysicsLayerMaskType layer)
    //{
    //    CollisionFilter filter = CollisionFilter.Default;
    //    filter.CollidesWith = uint.MaxValue;
    //    filter.BelongsTo = (uint)layer;
    //    return filter;
    //}

    public static CollisionFilter MakeEverythingFilter()
    {
        CollisionFilter filter = CollisionFilter.Default;
        filter.CollidesWith = uint.MaxValue;
        filter.BelongsTo = uint.MaxValue;
        return filter;
    }

    public static CollisionFilter MakeFilter(uint belongsTo, uint collidesWith)
    {
        CollisionFilter filter = CollisionFilter.Default;
        filter.CollidesWith = collidesWith;
        filter.BelongsTo = belongsTo;
        return filter;
    }

    //public static CollisionFilter MakeFilter(PhysicsLayerMaskType belongsTo, PhysicsLayerMaskType collidesWith)
    //{
    //    CollisionFilter filter = CollisionFilter.Default;
    //    filter.CollidesWith = (uint)collidesWith;
    //    filter.BelongsTo = (uint)belongsTo;
    //    return filter;
    //}

    //// Perform a terrain stab (raycast) into an ECS terrain from the camera using the given screen position
    //// It ONLY checks the terrain and ignores everything else (use Pick() if you want to hunt for other entities as well)
    ////   return --> returns true if the terrain was hit and sets hitPos to the resulting position
    ////   screenPos --> the position on the screen that you want to project to world coordinates
    ////   hit --> the result of the terrain stab (out param)
    ////   cam --> the camera to use (start of the ray)
    ////   terrainCollider --> the terrain to check against. See note at top of this file for the best way to fetch the terrain collider.
    //// *** if you want to find the distance between from the beginning of the ray to the hit object use: hit.Fraction * 1000
    //public unsafe static bool TerrainStab(in UnityEngine.Vector2 screenPos, out RaycastHit hit, UnityEngine.Camera cam, in PhysicsCollider terrainCollider)
    //{
    //    CollisionFilter filter = MakeFilter(PhysicsLayerMaskType.Everything, PhysicsLayerMaskType.Terrain);
    //    UnityEngine.Ray ray = cam.ScreenPointToRay(screenPos);
    //    RaycastInput raycastInput = new RaycastInput() { Start = ray.origin, End = ray.origin + (ray.direction * 1000), Filter = filter };
    //    unsafe {
    //        MeshCollider* pTerrainCollider = (MeshCollider*)terrainCollider.ColliderPtr;
    //        //TerrainCollider* pTerrainCollider = (TerrainCollider*)terrainCollider.ColliderPtr;
    //        return pTerrainCollider->CastRay(raycastInput, out hit);
    //    }
    //}

    //// same as above, except the current pointer position is used (via unity's new input system)
    //public unsafe static bool TerrainStab(out RaycastHit hit, UnityEngine.Camera cam, in PhysicsCollider terrainCollider)
    //{
    //    return TerrainStab(UnityEngine.InputSystem.Pointer.current.position.ReadValue(), out hit, cam, terrainCollider);
    //}

    //// same as above overload, except this version runs a little slower because it must fetch some default things first
    //public static bool TerrainStabDefaultWorld(in UnityEngine.Vector2 screenPos, out RaycastHit hit)
    //{
    //    unsafe {
    //        return TerrainStab(screenPos, out hit, UnityEngine.Camera.main, FindTerrainColliderFromDefaultWorld());
    //    }
    //}

    //// same as above overload, but the current pointer position is used (via unity's new input system)
    //// it is slightly slower because it must fetch some default things first
    //public static bool TerrainStabDefaultWorld(out RaycastHit hit)
    //{
    //    return TerrainStabDefaultWorld(UnityEngine.InputSystem.Pointer.current.position.ReadValue(), out hit);
    //}

    //// perform a terrain stab using the specified ray
    //// It ONLY checks the terrain (use Pick() if you want to hunt for other entities as well)
    ////   rayStart --> the start point of the ray
    ////   rayDirection --> the ray's direction (preferably normalized or at least 1 or larger for each component)
    ////   hit --> the result
    ////   returns --> true or false depending on whether a hit occurred or not
    //public unsafe static bool TerrainStab(in float3 rayStart, in float3 rayDirection, out RaycastHit hit, in PhysicsCollider terrainCollider, float rayLength = 1000)
    //{
    //    CollisionFilter filter = MakeFilter(PhysicsLayerMaskType.Everything, PhysicsLayerMaskType.Terrain);
    //    RaycastInput raycastInput = new RaycastInput() { Start = rayStart, End = rayStart + (rayDirection * rayLength), Filter = filter };
    //    unsafe {
    //        MeshCollider* pTerrainCollider = (MeshCollider*)terrainCollider.ColliderPtr;
    //        //TerrainCollider* pTerrainCollider = (TerrainCollider*)terrainCollider.ColliderPtr;
    //        return pTerrainCollider->CastRay(raycastInput, out hit);
    //    }
    //}

    //// same as above overload, except this runs slightly slower because it must fetch the default world
    //public static bool TerrainStabDefaultWorld(in float3 rayStart, in float3 rayDirection, out RaycastHit hit, float rayLength = 1000)
    //{
    //    return TerrainStab(rayStart, rayDirection, out hit, FindTerrainColliderFromDefaultWorld(), rayLength);
    //}

    // This will cast a ray from the specified screen position into the world and tell you what entity it hit
    public static bool Pick(in UnityEngine.Vector2 screenPos, out RaycastHit hit, in CollisionWorld world, UnityEngine.Camera cam)
    {
        CollisionFilter filter = MakeEverythingFilter();
        UnityEngine.Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastInput raycastInput = new RaycastInput() { Start = ray.origin, End = ray.origin + (ray.direction * 1000), Filter = filter };
        return world.CastRay(raycastInput, out hit);
    }

    //// same as the above version, except this uses the current pointer position (via unit's new input system)
    //public static bool Pick(out RaycastHit hit, in CollisionWorld world, UnityEngine.Camera cam)
    //{
    //    return Pick(UnityEngine.InputSystem.Pointer.current.position.ReadValue(), out hit, world, cam);
    //}


    //// same as above overload, except this version is a little slower because it must fetch the main camera and default world
    //public static bool PickDefaultWorld(in UnityEngine.Vector2 screenPos, out RaycastHit hit)
    //{
    //    return Pick(screenPos, out hit, GetDefaultBuildPhysicsWorld().PhysicsWorld.CollisionWorld, UnityEngine.Camera.main);
    //}


    //// same as the above overload, except the current pointer position is used for the screen position (using unity's new input system)
    //// it runs a little slower because it must fetch some defaults
    //public static bool PickDefaultWorld(out RaycastHit hit)
    //{
    //    return PickDefaultWorld(UnityEngine.InputSystem.Pointer.current.position.ReadValue(), out hit);
    //}

    //// enable/disable collisions for the specified collider and layer
    //public static void ToggleFilterCollisions(in PhysicsCollider collider, PhysicsLayerMaskType layer, bool enable)
    //{
    //    unsafe {
    //        CollisionFilter filter = collider.ColliderPtr->GetCollisionFilter();
    //        filter.CollidesWith = BitmaskUtils.ToggleBits(filter.CollidesWith, (uint)layer, enable);
    //        collider.ColliderPtr->SetCollisionFilter(filter);
    //    }
    //}

    // set the collision filter for the specified collider
    // returns the previous collides-with value
    public static uint SetCollisionFilter(in PhysicsCollider collider, uint collidesWith = uint.MaxValue)
    {
        uint ret = 0;
        unsafe {
            CollisionFilter filter = collider.ColliderPtr->GetCollisionFilter();
            ret = filter.CollidesWith;
            filter.CollidesWith = collidesWith;
            collider.ColliderPtr->SetCollisionFilter(filter);
        }
        return ret;
    }

    //public static void ToggleCollisionsWithTerrain(ref PhysicsCollider collider, bool enable)
    //{
    //    unsafe {
    //        CollisionFilter filter = collider.ColliderPtr->GetCollisionFilter();
    //        filter.CollidesWith = enable ? filter.CollidesWith | (uint)PhysicsLayerMaskType.Terrain : filter.CollidesWith ^ (uint)PhysicsLayerMaskType.Terrain;
    //        collider.ColliderPtr->SetCollisionFilter(filter);
    //    }
    //}

    // set the specified material for the given collider
    public static void SetColliderMaterial(in PhysicsCollider collider, Material material)
    {
        unsafe {
            switch (collider.ColliderPtr->Type) {
                case ColliderType.Box: ((BoxCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Sphere: ((SphereCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Convex: ((ConvexCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Capsule: ((CapsuleCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Triangle: ((PolygonCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Quad: ((PolygonCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Cylinder: ((CylinderCollider*)collider.ColliderPtr)->Material = material; break;
                case ColliderType.Terrain: ((TerrainCollider*)collider.ColliderPtr)->Material = material; break;
            }
        }
    }

    // fetches the current material for the specified collider
    public static Material GetColliderMaterial(in PhysicsCollider collider)
    {
        unsafe {
            switch (collider.ColliderPtr->Type) {
                case ColliderType.Box: return ((BoxCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Sphere: return ((SphereCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Convex: return ((ConvexCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Capsule: return ((CapsuleCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Triangle: return ((PolygonCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Quad: return ((PolygonCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Cylinder: return ((CylinderCollider*)collider.ColliderPtr)->Material;
                case ColliderType.Terrain: return ((TerrainCollider*)collider.ColliderPtr)->Material;
            }
            // NOTE - MESH AND COMPOUND DON'T HAVE A MATERIAL
        }
        return Material.Default;
    }

    // the collider MUST be of box type
    public static unsafe BoxCollider* GetBoxColliderPtr(in PhysicsCollider collider)
    {
        return (BoxCollider*)collider.ColliderPtr;
    }

    // sets the collision response policy for the specified collider and returns the old policy
    public static CollisionResponsePolicy SetCollisionResponsePolicy(in PhysicsCollider collider, CollisionResponsePolicy policy)
    {
        CollisionResponsePolicy oldPolicy = CollisionResponsePolicy.Collide;
        unsafe {
            Material material = GetColliderMaterial(collider);
            oldPolicy = material.CollisionResponse;
            material.CollisionResponse = policy; //material.CollisionResponse |= policy;
            SetColliderMaterial(collider, material);
        }
        return oldPolicy;
    }

    public static void ToggleMassFactorsFlag(in PhysicsCollider collider, bool enable)
    {
        unsafe {
            Material material = GetColliderMaterial(collider);
            material.EnableMassFactors = enable;
            SetColliderMaterial(collider, material);
        }
    }

    // overloaded version for use if you don't already have the collider
    public static void ToggleMassFactorsFlag(EntityManager em, in Entity entity, bool enable)
    {
        ToggleMassFactorsFlag(em.GetComponentData<PhysicsCollider>(entity), enable);
    }

    public static CollisionResponsePolicy ToggleCollisionResponse(in PhysicsCollider collider, bool enable)
    {
        return SetCollisionResponsePolicy(collider, enable ? CollisionResponsePolicy.Collide : CollisionResponsePolicy.None);
    }

    //// ensures that the given previous and next position won't intersect with the terrain
    //// (so that it is at least minTerrainHoverDistance units away from the terrain)
    ////   oldPos --> the current position
    ////   newPos --> the position you're considering moving to
    ////   minTerrainHoverDistance --> the closest distance (in meters) that the new position is allowed to get to the terrain
    ////   terrainCollider --> the collider to check against
    ////   return --> the clamped position
    //unsafe public static UnityEngine.Vector3 ClampToTerrain(in UnityEngine.Vector3 oldPos, in UnityEngine.Vector3 newPos, in float minTerrainHoverDistance, in PhysicsCollider terrainCollider)
    //{
    //    UnityEngine.Vector3 direction = UnityEngine.Vector3.Normalize(newPos - oldPos); // the direction vector is the new position minus the previous one (normalized)

    //    // shoot out a short ray that is min-terrain-hover-distance units long in the direction of movement. 
    //    // if there is a hit, then we know the newPos would collide
    //    if (TerrainStab(oldPos, direction, out RaycastHit hit, terrainCollider, minTerrainHoverDistance)) {
    //        // if execution get here, it's because the distance-to-terrain in the direction of travel is going to cause the camera to get too close to the terrain
    //        // TODO: i don't know if it's worth it, but we could project along the direction such that the camera would move to exactly min-terrain-hover distance from the terrain.
    //        return oldPos;
    //    }
    //    return newPos;
    //}

    //// same as above except it uses tghe default world to get the terrain collider
    //public static UnityEngine.Vector3 ClampToTerrainDefaultWorld(in UnityEngine.Vector3 oldPos, in UnityEngine.Vector3 newPos, in float minTerrainHoverDistance)
    //{
    //    UnityEngine.Vector3 direction = UnityEngine.Vector3.Normalize(newPos - oldPos); // the direction vector is the new position minus the previous one (normalized)

    //    // shoot out a short ray that is min-terrain-hover-distance units long in the direction of movement. 
    //    // if there is a hit, then we know the newPos would collide
    //    if (TerrainStabDefaultWorld(oldPos, direction, out RaycastHit hit, minTerrainHoverDistance)) {
    //        // if execution get here, it's because the distance-to-terrain in the direction of travel is going to cause the camera to get too close to the terrain
    //        // TODO: i don't know if it's worth it, but we could project along the direction such that the camera would move to exactly min-terrain-hover distance from the terrain.
    //        return oldPos;
    //    }
    //    return newPos;
    //}

    // returns the number of results
    public static int FindBodiesInAabbCube(in CollisionWorld world, in CollisionFilter filter, in float3 pos, float range, ref NativeList<int> results)
    {
        OverlapAabbInput overlapInput;
        // filter
        overlapInput.Filter = filter;
        // bounding box
        float3 radius3 = new float3(range, range, range);
        Aabb aabb;
        aabb.Max = pos + radius3;
        aabb.Min = pos - radius3;
        overlapInput.Aabb = aabb;
        // perform overlap calculation
        world.OverlapAabb(overlapInput, ref results);
        return results.Length;
    }

    public static OverlapAabbInput MakeOverlapAabbInput(in CollisionFilter filter, in float3 pos, float radius)
    {
        OverlapAabbInput input;
        input.Filter = filter;
        float3 radius3 = new float3(radius, radius, radius);
        input.Aabb = new Aabb { Min = pos - radius3, Max = pos + radius3 };
        return input;
    }

    // modifies results such that it only contains body indexes that are within the specified radius centered at the specified position.
    // it removes by swapping to back so it should be pretty fast
    // typically you would call FindBodiesInAabbCube() and then this after
    public static int TrimBodiesToSphere(in CollisionWorld world, in float3 pos, float radius, ref NativeList<int> results)
    {
        int count = 0;
        float radiusSq = radius * radius;
        for (int i = 0; i < results.Length; i++) {
            float3 hitPos = world.Bodies[results[i]].WorldFromBody.pos;
            if (math.distancesq(hitPos, pos) <= radiusSq) {
                results.RemoveAtSwapBack(i);
                i--; // pos i is now the item that was at the back, so decrementing i causes the formerly-last item to be examined
            }
        }
        return count;
    }

    // returns the closest entity in the specified sphere that matches the given collision filter
    public static EntityHit FindNearestEntityInAabbSphere(in CollisionWorld world, in CollisionFilter filter, in float3 pos, float radius)
    {
        NativeList<int> results = new NativeList<int>(Allocator.Temp);
        FindBodiesInAabbCube(world, filter, pos, radius, ref results);
        float radiusSq = radius * radius;
        Entity nearestEntity = Entity.Null;
        float minDistSq = float.MaxValue;
        for (int i = 0; i < results.Length; i++) {
            float3 hitPos = world.Bodies[results[i]].WorldFromBody.pos;
            float distSq = math.distancesq(hitPos, pos);
            if ((distSq <= radiusSq) && (distSq < minDistSq)) {
                minDistSq = distSq;
                nearestEntity = world.Bodies[results[i]].Entity;
            }
        }
        EntityHit result;
        result.distance = (nearestEntity == Entity.Null) ? float.MaxValue : math.sqrt(minDistSq);
        result.entity = nearestEntity;
        results.Dispose();
        return result;
    }

    //// returns the bounds (rectangle) of the specified collider at ground level
    //// the specified collider must be a box collider
    //public static UnityEngine.Bounds GetGroundBounds(in PhysicsCollider collider, float3 pos, PhysicsCollider terrainCollider)
    //{
    //    // fetch the geometry of the collider (which is in local coordinates)
    //    BoxGeometry geo;
    //    unsafe {
    //        BoxCollider* pBoxCollider = (BoxCollider*)collider.ColliderPtr;
    //        geo = pBoxCollider->Geometry; // note - Transform.Scale or NonUniformScale is already applied to this
    //    }

    //    // get the center in world coordinates
    //    float3 center = pos + geo.Center;

    //    // get the center position on the ground
    //    // do a terrain stab starting from a point slightly above the building straight down.
    //    if (!TerrainStab(center + (math.up() * 4f), math.down(), out RaycastHit hitGround, terrainCollider, 20f)) {
    //        //float3 start = center + (math.up() * 4f);
    //        //DebugDrawUtils.DrawLine(start, start + (math.down() * 1000), UnityEngine.Color.magenta, 6f);
    //        return new UnityEngine.Bounds();
    //    }

    //    float3 groundedCenter = hitGround.Position;
    //    UnityEngine.Vector3 size = geo.Size;
    //    return new UnityEngine.Bounds(groundedCenter, size);
    //}

    //// returns the bounds (rectangle) of the specified entity's collision box at ground level
    //// the specified entity must have a box collider
    //public static UnityEngine.Bounds GetGroundBounds(EntityManager em, in Entity entity, in PhysicsCollider terrainCollider)
    //{
    //    PhysicsCollider collider = em.GetComponentData<PhysicsCollider>(entity);
    //    float3 pos = em.GetComponentData<Unity.Transforms.LocalToWorld>(entity).Position;
    //    return GetGroundBounds(collider, pos, terrainCollider);
    //}

    public static float3 GetBoxColliderSize(in PhysicsCollider collider, in float3 scale)
    {
        unsafe  {
            BoxCollider* pBox = ((BoxCollider*)collider.ColliderPtr);
            return pBox->Size * scale;
        }
    }

    public static float CalculateColliderHeightRadius(in PhysicsCollider collider, float3 scale)
    {
        scale = 1.0f / scale;
        unsafe {
            switch (collider.ColliderPtr->Type) {
                case ColliderType.Box: return ((BoxCollider*)collider.ColliderPtr)->Size.y * scale.y * 0.5f;
                case ColliderType.Sphere: return ((SphereCollider*)collider.ColliderPtr)->Radius * scale.y;
                case ColliderType.Capsule: return math.abs(((CapsuleCollider*)collider.ColliderPtr)->Vertex0.y - ((CapsuleCollider*)collider.ColliderPtr)->Vertex1.y) * scale.y * 0.5f;
                case ColliderType.Cylinder: return ((CylinderCollider*)collider.ColliderPtr)->Height * scale.y * 0.5f;
            }
        }
        return 0f;
    }

    // fetches the "radius" of the collider - note this is the XZ radius
    // scale is the Scale/NonUniformScale/ComponentScale component from the entity that has the collider
    // for boxes, this is the distance from the two farthest corners (in the xz plane)
    // for spheres the radius is obvious, however it could be ovular if the scale is non-uniform, in which case the radius will be the longest part
    // for capsules and cylinders the radius is the x/z radius, however it could be ovular if the scale is non-uniform, in which case the radius will be the longest part
    public static float CalculatePrimitiveColliderRadiusXZ(in PhysicsCollider collider, float3 scale)
    {
        scale = 1.0f / scale;
        unsafe {
            switch (collider.ColliderPtr->Type) {
                case ColliderType.Box: 
                    BoxCollider* pBox = ((BoxCollider*)collider.ColliderPtr);
                    return math.distance(float2.zero, new float2(pBox->Size.x * 0.5f * scale.x, pBox->Size.z * 0.5f * scale.z));
                    //return math.distance(new float2(pBox->Center.x * scale.x, pBox->Center.z * scale.z), new float2(pBox->Size.x * 0.5f * scale.x, pBox->Size.z * 0.5f * scale.z));
                case ColliderType.Sphere: return ((SphereCollider*)collider.ColliderPtr)->Radius * math.max(scale.x, scale.z);
                case ColliderType.Capsule: return ((CapsuleCollider*)collider.ColliderPtr)->Radius * math.max(scale.x, scale.z);
                case ColliderType.Cylinder: return ((CylinderCollider*)collider.ColliderPtr)->Radius * math.max(scale.x, scale.z);
            }
        }
        return 0f;
    }

    public static PhysicsCollider MakeCapsuleCollider()
    {
        CollisionFilter filter = MakeEverythingFilter();
        Material mat = new Material
        {
            CollisionResponse = CollisionResponsePolicy.Collide,
            EnableMassFactors = true,
            EnableSurfaceVelocity = false,
            CustomTags = 0,
            Friction = 0.5f,
            FrictionCombinePolicy = Material.CombinePolicy.GeometricMean,
            Restitution = 0f,
            RestitutionCombinePolicy = Material.CombinePolicy.Maximum
        };
        CapsuleGeometry capsule = new CapsuleGeometry { Radius = 0.5f, Vertex0 = new float3(0f, -1f, 0f), Vertex1 = new float3(0f, 1f, 0f)};
        return new PhysicsCollider { Value = CapsuleCollider.Create(capsule, filter, mat) };
    }

    public static void AddPhysicsComponents(EntityManager em, Entity entity, in PhysicsCollider collider)
    {
        em.AddComponentData(entity, collider);
        em.AddComponentData(entity, new PhysicsDamping { Angular = 2f, Linear = 1f });
        em.AddComponentData(entity, PhysicsMass.CreateDynamic(collider.MassProperties, 1f));
        //em.AddComponentData(entity, new PhysicsMass { Transform = new RigidTransform { pos = new float3(0,0,0), rot = new quaternion(0,0,0,1) }, AngularExpansionFactor = 0, InverseInertia = new float3(10,10,10), InverseMass = 1f });
        em.AddComponent<PhysicsVelocity>(entity);
        em.AddComponent<PhysicsWorldIndex>(entity);
    }

    public static void AddPhysicsComponents(EntityManager em, in NativeArray<Entity> entities, in PhysicsCollider collider)
    {
        for (int i = 0; i < entities.Length; i++) {
            AddPhysicsComponents(em, entities[i], collider);
        }
    }

    public static void RemovePhysicsComponents(EntityManager em, Entity entity)
    {
        em.RemoveComponent<PhysicsCollider>(entity);
        em.RemoveComponent<PhysicsDamping>(entity);
        em.RemoveComponent<PhysicsMass>(entity);
        em.RemoveComponent<PhysicsVelocity>(entity);
        em.RemoveComponent<PhysicsWorldIndex>(entity);
    }

    public static void RemovePhysicsComponents(EntityManager em, in NativeArray<Entity> entities)
    {
        for (int i = 0; i < entities.Length; i++) {
            RemovePhysicsComponents(em, entities[i]);
        }
    }
}