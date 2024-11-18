using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
public class ChampAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float jumpAirTimeMax;
    public float jumpStrength;
    public Vector3 gravityDirection;
    public float gravityStrength;
    public float gravityStrengthMax;
    public float gravityAirTimeMax;
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
            AddComponent(entity, new JumpState {
                State = 0,
                AirTime = 0,
                AirTimeMax = authoring.jumpAirTimeMax
            });
            AddComponent(entity, new JumpVelocity {Value = float3.zero});
            AddComponent(entity, new JumpStrength {Value = authoring.jumpStrength});

            //Ground check components
            AddComponent(entity, new GroundCheck{AirTime = 0});

            //Gravity components
            AddComponent(entity, new GravityVelocity {Value = float3.zero});
            AddComponent(entity, new Gravity
            {
                Direction = authoring.gravityDirection,
                Strength = authoring.gravityStrength,
                StrengthMax = authoring.gravityStrengthMax,
                AirTime = 0,
                AirTimeMax = authoring.gravityAirTimeMax
            });
        }
    }
}
