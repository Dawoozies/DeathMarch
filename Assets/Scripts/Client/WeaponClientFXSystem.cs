using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine.VFX;
public partial class WeaponClientFXSystem : SystemBase
{
    protected override void OnCreate()
    {
    }
    protected override void OnUpdate()
    {
        Entities.ForEach((VisualEffect vfx, in BulletLineFXEvent vfxEvent) =>
        {
            UnityEngine.Debug.Log("RUNNING VFX");
            // Set VFX parameters and play the effect
            vfx.SetVector3("StartPosition", vfxEvent.WeaponFiringPoint);
            VFXEventAttribute shootDirectionAttribute = vfx.CreateVFXEventAttribute();
            shootDirectionAttribute.SetVector3("ShootDirection", vfxEvent.ShootDirection);
            vfx.SendEvent("OnPlay", shootDirectionAttribute);

        }).WithoutBurst().Run();
    }
}