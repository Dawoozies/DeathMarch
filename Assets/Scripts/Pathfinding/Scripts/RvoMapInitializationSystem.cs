// This system adds entities to RVO sim and map after they are first created.
// Note that IJobEntity cannot be used here because I need access to ISimulation, which is an interface,
// and IJobEntity cannot use non-value types (it will throw: "error SGJE0009: RvoMapInitializationSystem contains non-value type fields.")

using Unity.Entities;
using Pathfinding.RVO;
using Unity.Transforms;
struct InitRvoAgentTag : IComponentData { }

[RequireMatchingQueriesForUpdate]
public partial class RvoMapInitializationSystem : SystemBase
{
    SimulatorBurst m_sim;
    bool hasRvo;
    protected override void OnStartRunning()
    {
        RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    protected override void OnUpdate()
    {
        var ecbEnd = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
        if(!hasRvo)
        {
            RVOSimulator simulator = RVOSimulator.active;
            if(simulator != null)
            {
                m_sim = simulator.GetSimulator();
                hasRvo = true;
            }
            if(!hasRvo)
            {
                return;
            }
        }
        // for every agent that has an InitRvoAgentTag on it...
        // note that because RadiusData is a parameter, we can be assured that this system will execute only after a radius has been calculated.
        Entities.WithAll<InitRvoAgentTag>().ForEach((Entity entity, int entityInQueryIndex, in LocalToWorld ltw) => {
            // add the agent to the simulation
            IAgent agent = m_sim.AddAgent(ltw.Position);
            agent.Radius = 0.5f * 0.65f;
            agent.Height = 0.5f * 2f;
            agent.Locked = false;
            agent.AgentTimeHorizon = 2;
            agent.ObstacleTimeHorizon = 2;
            agent.MaxNeighbours = 10;
            agent.Layer = RVOLayer.DefaultAgent;
            agent.CollidesWith = (RVOLayer)(-1);
            agent.Priority = 0.5f;
            agent.FlowFollowingStrength = 0.1f;
            agent.MovementPlane = Pathfinding.Util.SimpleMovementPlane.XZPlane;
            //agent.DebugDraw = false;

            ecbEnd.AddComponent(entityInQueryIndex, entity, new RvoAgentData { agentIndex = agent.AgentIndex });

            //RvoMapManager.Instance.map.Add(agent.AgentIndex, entity);

            // ensure that this system won't run again for this entity
            ecbEnd.RemoveComponent<InitRvoAgentTag>(entityInQueryIndex, entity);
        }).WithoutBurst().Run();
    }
}