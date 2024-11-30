using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class RotateWithMouseDeltaAuthoring : MonoBehaviour
{
    public Vector3 axisMultipliers;
    public class RotateWithMouseDeltaBaker : Baker<RotateWithMouseDeltaAuthoring>
    {
        public override void Bake(RotateWithMouseDeltaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RotateWithMouseDeltaTag>(entity);
            AddComponent<OriginalRotation>(entity, new OriginalRotation
            {
                Value = new float4(
                    authoring.transform.localRotation.x,
                    authoring.transform.localRotation.y,
                    authoring.transform.localRotation.z,
                    authoring.transform.localRotation.w
                    )
            });
            AddComponent<RotationAxis>(entity, new RotationAxis
            {
                Value = authoring.axisMultipliers
            });
            AddComponent<MouseDeltaRotationAngles>(entity, new MouseDeltaRotationAngles
            {
                Value = float2.zero
            });
        }
    }
}