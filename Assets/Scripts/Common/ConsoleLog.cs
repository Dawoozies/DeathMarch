using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
public static class ConsoleLog
{
    public static void Log(string text, EntityManager entityManager, Entity entity)
    {
        if(entityManager.WorldUnmanaged.IsServer())
        {
            Debug.LogError($"ENTITY:{entity.Index}:{entity.Version} | SERVER_LOG:{text}");
        }
        if(entityManager.WorldUnmanaged.IsClient())
        {
            Debug.LogError($"ENTITY:{entity.Index}:{entity.Version} | CLIENT_LOG:{text}");
        }
    }
}