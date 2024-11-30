// this is a static utility class that contains useful entity functions
// its functions are generic for any code using unity packages (does not rely on any externally defined components)
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public struct EntityHit
{
    public Entity entity;
    public float distance;
}


public static class EntityUtils
{
    //// fetches the value for the specified singleton type and entity manager.
    //// will throw an error if the instance count of the specified type is not exactly one
    //public static T QuerySingleton<T>(EntityManager em) where T : struct, IComponentData { return em.CreateEntityQuery(ComponentType.ReadOnly<T>()).GetSingleton<T>(); }

    // fetches a previously created singleton value using the default world
    // will throw an error if the instance count of the specified type is not exactly one
    public static T QueryDefaultWorldSingleton<T>() where T : unmanaged, IComponentData
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>()).GetSingleton<T>();
    }

    // same as QueryDefaultWorldSingleton(), but with a way to detect a failed query
    public static bool TryQueryDefaultWorldSingleton<T>(out T val) where T : unmanaged, IComponentData
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>()).TryGetSingleton(out val);
    }

    // returns the singleton entity with the specified type, and returns Entity.NULL if the query fails
    public static Entity QueryDefaultWorldSingletonEntity<T>() where T : struct, IComponentData
    {
        Entity retEntity = Entity.Null;
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>()).TryGetSingletonEntity<T>(out retEntity);
        return retEntity;
    }

    // fetches a previously created singleton value using the default world
    // will throw an error if the instance count of the specified type is not exactly one
    public static T QuerySingleton<T>(EntityManager em) where T : unmanaged, IComponentData
    {
        return em.CreateEntityQuery(ComponentType.ReadOnly<T>()).GetSingleton<T>();
    }

    public static T QuerySingleton<T>(ref SystemState state) where T : unmanaged, IComponentData
    {
        return state.GetEntityQuery(ComponentType.ReadOnly<T>()).ToComponentDataArray<T>(Allocator.Temp)[0];
    }

    // same as QueryDefaultWorldSingleton(), but with a way to detect a failed query
    public static bool TryQuerySingleton<T>(EntityManager em, out T val) where T : unmanaged, IComponentData
    {
        return em.CreateEntityQuery(ComponentType.ReadOnly<T>()).TryGetSingleton(out val);
    }

    // returns the singleton entity with the specified type, and returns Entity.NULL if the query fails
    public static Entity QuerySingletonEntity<T>(EntityManager em) where T : struct, IComponentData
    {
        Entity retEntity = Entity.Null;
        em.CreateEntityQuery(ComponentType.ReadOnly<T>()).TryGetSingletonEntity<T>(out retEntity);
        return retEntity;
    }

    // returns the singleton entity with the specified type, and returns Entity.NULL if the query fails
    public static Entity QuerySingletonEntity<T>(ref SystemState state) where T : struct, IComponentData
    {
        return state.GetEntityQuery(ComponentType.ReadOnly<T>()).ToEntityArray(Allocator.Temp)[0];
    }

    //// sets a previously created singleton to the specified value using the dfeault world
    //public static void SetDefaultWorldSingleton<T>(T value) where T : struct, IComponentData { World.DefaultGameObjectInjectionWorld.Systems[0].SetSingleton(value); }

    //// sets a previously created singleton value to the specified value in the specified world
    //public static void SetSingleton<T>(World w, T value) where T : struct, IComponentData { w.Systems[0].SetSingleton(value); }

    // returns true if one or more instances of the specified entity exists in the specified entity manager's world
    public static bool Exists<T>(EntityManager em) { return em.CreateEntityQuery(ComponentType.ReadOnly<T>()).CalculateChunkCount() > 0; }

    // returns true if one or more instances of the specified entity exists in the default world
    public static bool ExistsInDefaultWorld<T>() { return World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>()).CalculateChunkCount() > 0; }

    // creates a singleton in the default world and sets its value to the specified value
    public static Entity CreateDefaultWorldSingleton<T>(T value) where T : unmanaged, IComponentData
    {
        Entity entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(typeof(T));
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, value);
        #if UNITY_EDITOR
            World.DefaultGameObjectInjectionWorld.EntityManager.SetName(entity, typeof(T).ToString());
        #endif
        return entity;
    }

    // creates a singleton in the default world
    public static Entity CreateDefaultWorldSingleton<T>() where T : struct, IComponentData
    {
        Entity entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity(typeof(T));
        #if UNITY_EDITOR
            World.DefaultGameObjectInjectionWorld.EntityManager.SetName(entity, typeof(T).ToString());
        #endif
        return entity;
    }


    // create a singleton using the specified entity manager and set its value
    public static Entity CreateSingleton<T>(EntityManager em, T value) where T : unmanaged, IComponentData
    {
        Entity entity = em.CreateEntity(typeof(T));
        em.AddComponentData(entity, value);
        #if UNITY_EDITOR
            em.SetName(entity, typeof(T).ToString());
        #endif
        return entity;
    }


    public static Entity CreateSingleton<T>(EntityManager em) where T : unmanaged, IComponentData
    {
        Entity entity = em.CreateEntity(typeof(T));
        #if UNITY_EDITOR
            em.SetName(entity, typeof(T).ToString());
        #endif
        return entity;
    }

    public static EntityManager GetDefaultWorldManager() { return World.DefaultGameObjectInjectionWorld.EntityManager; }
}