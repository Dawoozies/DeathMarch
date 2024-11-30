// This is a basic singleton.
// Inherit from it and add it to a single game-object in the scene.
// Setting persistent to true will allow it to remain between scenes.
//
// Example of use:
//   public class MyClass : Singleton<PathController> { }

using UnityEngine;

public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{
    private static T m_instance;

    [SerializeField]
    private bool m_persistentSingleton = false;

    public static T Instance
    {
        get {
            if (m_instance != null) { return m_instance; }
            var instances = FindObjectsByType<T>(FindObjectsSortMode.None);
            var count = instances.Length;
            if (count > 0) {
                if (count == 1) { return m_instance = instances[0]; }
                Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                for (var i = 1; i < instances.Length; i++) { DestroyImmediate(instances[i]); }
                return m_instance = instances[0];
            }

            //Debug.Log($"[{nameof(Singleton)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
            return m_instance = new GameObject($"({nameof(Singleton)}){typeof(T)}").AddComponent<T>();
        }
    }

    private void Awake()
    {
        if (m_persistentSingleton) { DontDestroyOnLoad(gameObject); }
        OnAwake();
    }

    protected virtual void OnAwake() { }
}

public abstract class Singleton : MonoBehaviour
{
    
}