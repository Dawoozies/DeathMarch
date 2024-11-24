using UnityEngine;
public class WorldCanvasSingleton : MonoBehaviour
{
    public static WorldCanvasSingleton ins;
    void Awake()
    {
        ins = this;
    }
}
