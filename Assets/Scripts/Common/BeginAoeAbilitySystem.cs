using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct BeginAoeAbilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //This BeginSimulationECB basically ensures we can run this update as close
        // as possible to the start of an update frame. We want this so things appear to
        //  render properly and happen in the correct order.
        BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();

        //full vs partial ticks
        //IsFirstTimeFullyPredictingTick =
        //  False on Partial ticks
        //  True on Full ticks
        //  False when resimulating Full tick
        //  Best used for events 
        //      - when it really matters when something HAS occurred
        //      - when the exact frame an event happened
        if(!networkTime.IsFirstTimeFullyPredictingTick)
        {
            return;
        }

        //We only want to get here if this is the first time FULLY predicting this tick
        NetworkTick currentTick = networkTime.ServerTick;
        foreach (AoeAspect aoe in SystemAPI.Query<AoeAspect>().WithAll<Simulate>())
        {
            bool isOnCooldown = true;
            AbilityCooldownTargetTicks curTargetTicks = new AbilityCooldownTargetTicks();

            for (uint i = 0; i < networkTime.SimulationStepBatchSize; i++)
            {
                NetworkTick testTick = currentTick;
                testTick.Subtract(i);

                if(!aoe.CooldownTargetTicks.GetDataAtTick(testTick, out curTargetTicks))
                {
                    curTargetTicks.AoeAbility = NetworkTick.Invalid;
                }
                if(curTargetTicks.AoeAbility == NetworkTick.Invalid || !curTargetTicks.AoeAbility.IsNewerThan(currentTick))
                {
                    isOnCooldown = false;
                    break;
                }
            }

            if(isOnCooldown)
                continue;

            //If the player has pressed the attack button in this current network tick
            if(aoe.ShouldAttack)
            {
                //instantiate new aoe ability
                Entity newAoeAbility = ecb.Instantiate(aoe.AbilityPrefab);
                LocalTransform abilityTransform = LocalTransform.FromPosition(aoe.AttackPosition);
                ecb.SetComponent(newAoeAbility, abilityTransform);
                ecb.SetComponent(newAoeAbility, aoe.Team);

                if(state.WorldUnmanaged.IsServer()) continue;

                NetworkTick newCooldownTargetTick = currentTick;
                newCooldownTargetTick.Add(aoe.CooldownTicks);
                curTargetTicks.AoeAbility = newCooldownTargetTick;

                NetworkTick nextTick = currentTick;
                nextTick.Add(1u);
                curTargetTicks.Tick = nextTick;

                aoe.CooldownTargetTicks.AddCommandData(curTargetTicks);
            }
        }
    }
}