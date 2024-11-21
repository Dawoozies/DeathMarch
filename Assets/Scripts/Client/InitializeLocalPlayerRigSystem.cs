using Rukhanka;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
public struct OwnedAnimatorTag : IComponentData {}
//public partial struct InitializeLocalPlayerSystem : ISystem
//{
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<NetworkId>();
//        state.RequireForUpdate<OwnerChampTag>();
//        state.RequireForUpdate<RigDefinitionComponent>();
//    }
//    public void OnUpdate(ref SystemState state)
//    {
//        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
//        Entity playerEntity = SystemAPI.GetSingletonEntity<OwnerChampTag>();
//        foreach (var (_, parent, _, entity) in SystemAPI.Query<LocalTransform, Parent, RigDefinitionComponent>().WithAll<GhostChildEntity>().WithNone<OwnedAnimatorTag>().WithEntityAccess())
//        {
//            if(parent.Value == playerEntity)
//            {
//                ecb.AddComponent<OwnedAnimatorTag>(entity);
//            }
//        }
//        ecb.Playback(state.EntityManager);
//    }
//}