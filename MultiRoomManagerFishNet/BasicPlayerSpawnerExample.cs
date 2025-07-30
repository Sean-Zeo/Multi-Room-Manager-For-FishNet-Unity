using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine.SceneManagement;

public class BasicPlayerSpawnerExample : NetworkBehaviour
{
    public NetworkObject roomPlayerPrefab;
    public Vector3 spawnPosition;

    //Spawn room player prefab after scene load
    public override void OnStartClient()
    {
        base.OnStartClient();
        SpawnNetworkPlayer(spawnPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnNetworkPlayer(Vector3 position, NetworkConnection conn = null)
    {
        // Instantiate the prefab on the server
        NetworkObject newObj = Instantiate(roomPlayerPrefab, position, Quaternion.identity);
        // Move the new object into the same scene as this player (room isolation)
        Scene playerScene = gameObject.scene;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(newObj.gameObject, playerScene);
        // Spawn the object over the network, with ownership given to the player by passing NetworkConnection (conn)
        base.Spawn(newObj.gameObject, conn);
    }
}
