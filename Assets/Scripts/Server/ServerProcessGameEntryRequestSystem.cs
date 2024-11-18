using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerProcessGameEntryRequestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MobaPrefabs>();
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<MobaTeamRequest, ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        var championPrefab = SystemAPI.GetSingleton<MobaPrefabs>().Champion;
        foreach (var (teamRequest, requestSource, requestEntity) in SystemAPI.Query<MobaTeamRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ecb.DestroyEntity(requestEntity);
            ecb.AddComponent<NetworkStreamInGame>(requestSource.SourceConnection);

            TeamType requestedTeamType = teamRequest.Value;
            if(requestedTeamType == TeamType.AutoAssign)
            {
                requestedTeamType = TeamType.Blue;
            }

            int clientId = SystemAPI.GetComponent<NetworkId>(requestSource.SourceConnection).Value;
            Debug.Log($"Server is assigning Client ID: {clientId} to the {requestedTeamType.ToString()} team.");

            Entity newChamp = ecb.Instantiate(championPrefab);
            ecb.SetName(newChamp, "Champion");
            float3 spawnPosition = new float3(0, 1, 0);

            switch (requestedTeamType)
            {
                case TeamType.Blue:
                    spawnPosition = new float3(-6f, 0.5f, 0f);
                    break;
                case TeamType.Red:
                    spawnPosition = new float3(-6f, 0.5f, 0f);
                    break;
                default:
                    continue;
            }

            LocalTransform newTransform = LocalTransform.FromPosition(spawnPosition);
            ecb.SetComponent(newChamp, newTransform);
            ecb.SetComponent(newChamp, new GhostOwner { NetworkId = clientId }); //set champ association to a particular client
            ecb.SetComponent(newChamp, new MobaTeam { Value = requestedTeamType }); //Set player team
            ecb.AppendToBuffer(requestSource.SourceConnection, new LinkedEntityGroup { Value = newChamp }); //if player disconnects this would automatically destroy the entity :O
        }
        ecb.Playback(state.EntityManager);
    }
}