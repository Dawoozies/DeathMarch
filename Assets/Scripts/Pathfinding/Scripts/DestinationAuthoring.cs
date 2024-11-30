using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct DestinationTag : IComponentData { }

public class DestinationAuthoring : MonoBehaviour
{
    
}

public class DestinationBaker : Baker<DestinationAuthoring>
{
    public override void Bake(DestinationAuthoring authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.None);
        AddComponent<DestinationTag>(e);
    }
}