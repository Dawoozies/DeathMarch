using UnityEngine;
using Unity.Entities;
using Pathfinding;
using Pathfinding.ECS;
using UnityEngine.SceneManagement;
public class HordeAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float climbSpeed;
    public float gravity;
    public MovementSettings movementSettings;
    public MovementControl movementControl;
    public AgentCylinderShape cylinderShape;
    public Pathfinding.ECS.AutoRepathPolicy autoRepathPolicy;
    [SerializeField]
    ManagedState managedState = new ManagedState
    {
        enableLocalAvoidance = false,
        pathfindingSettings = PathRequestSettings.Default,
    };
    public OrientationMode orientationMode;
    public MovementPlaneSource movementPlaneSource;
    public bool syncPosition;
    public bool syncRotation;
    public class ComponentBaker : Baker<HordeAuthoring>
    {
        public override void Bake(HordeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HordeMove 
            {
                MoveSpeed = authoring.moveSpeed,
                ClimbSpeed = authoring.climbSpeed,
                Gravity = authoring.gravity
            });
            AddComponent(entity, new GroundCheck
            {
                AirTime = 0
            });
        }
    }
}