using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
public class ChampAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float jumpAirTimeMax;
    public Vector3 gravityDirection;
    public float gravityStrength;
    public class ChampBaker : Baker<ChampAuthoring>
    {
        public override void Bake(ChampAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ChampTag>(entity);
            AddComponent<NewChampTag>(entity);
            AddComponent<MobaTeam>(entity);
            AddComponent<URPMaterialPropertyBaseColor>(entity);
            AddComponent<AbilityInput>(entity);

            //Look to mouse components
            AddComponent<PlayerLookInput>(entity);

            //Planar Movement components
            AddComponent<PlayerMoveInput>(entity);
            AddComponent(entity, new MoveSpeed {Value = authoring.moveSpeed});
            AddComponent(entity, new MoveVelocity {Value = float3.zero});

            //Jump components
            AddComponent<PlayerJumpInput>(entity);
            AddComponent<JumpAirTimeMax>(entity);
            AddComponent<JumpAirTime>(entity);
            AddComponent<JumpState>(entity);
            AddComponent(entity, new JumpVelocity {Value = float3.zero});

            //Ground check components
            AddComponent(entity, new GroundCheck{isGrounded = false});

            //Gravity components
            AddComponent(entity, new GravityVelocity {Value = float3.zero});
            AddComponent(entity, new GravityDirection {Value = authoring.gravityDirection});
            AddComponent(entity, new GravityStrength {Value = authoring.gravityStrength});
        }
    }
}
