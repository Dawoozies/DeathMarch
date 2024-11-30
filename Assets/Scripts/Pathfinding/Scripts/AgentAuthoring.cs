// place this onto an agent to make it an agent that can be controlled and moved on A*PFP's nav mesh.

using Unity.Entities;
using UnityEngine;

public class AgentAuthoring : MonoBehaviour
{
    public float MoveSpeed = 15f;
    public bool UseRvo = true;
}

public class AgentBaker : Baker<AgentAuthoring>
{
    public override void Bake(AgentAuthoring authoring)
    {
        Entity agentEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(agentEntity, new MoveSpeedData { Speed = authoring.MoveSpeed });
        AddComponent(agentEntity, new MaxMoveSpeedData { Speed = authoring.MoveSpeed });
        AddComponent(agentEntity, new WaypointIndexData { Index = 0 });
        AddComponent(agentEntity, new TravelStateData { HasReachedEndOfPath = false });
        AddBuffer<WaypointData>(agentEntity);
        if (authoring.UseRvo) { AddComponent<InitRvoAgentTag>(agentEntity); }
    }
}