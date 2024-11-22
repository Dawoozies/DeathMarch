using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct InitializeLocalChampSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (transform, entity) in SystemAPI.Query<LocalTransform>().WithAll<GhostOwnerIsLocal>().WithNone<OwnerChampTag>().WithEntityAccess())
        {
            //THIS ONLY PUTS OWNER CHAMP TAG ONTO THE CLIENT SIDE PLAYER
            ecb.AddComponent<OwnerChampTag>(entity);
            //SO MAKING CHECKS AROUND THIS RESULT IN ONLY CLIENT SIDE STUFF
        }
        ecb.Playback(state.EntityManager);
    }
}