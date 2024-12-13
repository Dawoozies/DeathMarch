using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Common.Authoring
{
    public class PhysicsMassOverrideAuthoring : MonoBehaviour
    {
        public bool isKinematic;
        private class PhysicsMassOverrideAuthoringBaker : Baker<PhysicsMassOverrideAuthoring>
        {
            public override void Bake(PhysicsMassOverrideAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PhysicsMassOverride>(entity, new PhysicsMassOverride
                {
                    IsKinematic = authoring.isKinematic ? (byte)1 : (byte)0
                });
            }
        }
    }
}