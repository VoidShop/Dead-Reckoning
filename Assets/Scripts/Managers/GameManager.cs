using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public override void OnNetworkSpawn()
    {

        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId)
    {
        var prefabHash = playerPrefab.GetHashCode();

        //var spawn = Instantiate(playerPrefab);
        //spawn.NetworkObject.SpawnWithOwnership(playerId);
        var spawn = Instantiate(playerPrefab);
        spawn.SpawnWithOwnership(playerId);

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MatchmakingService.LeaveLobby();
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
    }
}
