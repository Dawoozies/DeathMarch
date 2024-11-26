using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager ins;
    void Awake()
    {
        ins = this;
    }
    public VisualEffect[] vfx;
    public VisualEffect GetEffect(int index)
    {
        if(index >= vfx.Length || index < 0)
        {
            Debug.LogError($"Trying to get VFX with index {index} which doesn't exist!");
            return null;
        }
        return vfx[index];
    }
}