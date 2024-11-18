using Unity.Entities;
using Unity.NetCode;
public struct MaxHitPoints : IComponentData
{
    public int Value;
}
public struct CurrentHitPoints : IComponentData
{
    [GhostField] public int Value;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct DamageBufferElement : IBufferElementData
{
    public int Value;
}
//Data is only synced from server to other predicted clients, not the local client.
//we know the damage value is correct for the local client but not the others
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public int Value;
}

public struct AbilityPrefabs : IComponentData
{
    public Entity AoeAbility;
    public Entity SkillShotAbility;
}
public struct DestroyOnTimer : IComponentData
{
    public float Value;
}
//Instead of messing with timers and shit and trying to sync all this data
//We instead on creation, calculate what tick we should destroy it at
//then once we reach that tick we can destroy
public struct DestroyAtTick : IComponentData
{
    [GhostField] public NetworkTick Value;
}
public struct DestroyEntityTag : IComponentData {}
public struct DamageOnTrigger : IComponentData
{
    public int Value;
}
public struct AlreadyDamagedEntity : IBufferElementData
{
    public Entity Value;
}
public struct AbilityCooldownTicks : IComponentData
{
    public uint AoeAbility;
    public uint SkillShotAbility;
}
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct AbilityCooldownTargetTicks : ICommandData
{
    public NetworkTick Tick { get; set; }
    public NetworkTick AoeAbility;
    public NetworkTick SkillShotAbility;
}
public struct AimSkillShotTag : IComponentData {}