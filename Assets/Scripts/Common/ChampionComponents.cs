using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
public struct ChampTag : IComponentData { }
public struct NewChampTag : IComponentData { }
public struct OwnerChampTag : IComponentData { }
public struct MobaTeam : IComponentData 
{
    [GhostField] public TeamType Value;
}

public struct CharacterMoveSpeed : IComponentData
{
    [GhostField(Quantization = 0)] public float Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct ChampMoveTargetPosition : IInputComponentData //Under the hood this is a DynamicBuffer
{
    [GhostField(Quantization = 0)] public float3 Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct AbilityInput : IInputComponentData
{
    //Input event = special NetCode type which fixes misinputs
    [GhostField] public InputEvent AoeAbility;
}
