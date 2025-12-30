using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class BasicPlayerSpawner : NetworkBehaviour
{
    public NetworkObject networkPlayerPrefab;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SpawnPlayer();
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer(NetworkConnection conn = null)
    {
        NetworkObject newPlayer = Instantiate(networkPlayerPrefab);
        Spawn(newPlayer, conn, gameObject.scene);
    }
}
