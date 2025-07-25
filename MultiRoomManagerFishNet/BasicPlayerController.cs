using UnityEngine;
using FishNet.Object;   
using FishNet.Connection;
using UnityEngine.SceneManagement;

public class BasicPlayerController : NetworkBehaviour
{
    public NetworkObject spawnablePrefab;   // Prefab to spawn (should have a NetworkObject component)
    public CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f; 

    private void Update()
    {
        // Enable controller only for the owning client’s player
        controller.enabled = base.IsOwner;

        // Disable controls for all players except the one owned by this client
        if (!base.IsOwner)
            return;

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Basic WASD movement
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = Vector3.ClampMagnitude(move, 1f); // prevent faster diagonal movement
        if (move != Vector3.zero)
        {
            transform.forward = move;
        }

        // Jump
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;

        // Move the character controller
        Vector3 finalMove = (move * playerSpeed) + (playerVelocity.y * Vector3.up);
        controller.Move(finalMove * Time.deltaTime);

        // Left-click spawns an example network object in the room
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = new Vector3(0, 2, 0);
            SpawnNetworkObjectExample(pos);
        }
    }

    [ServerRpc]
    private void SpawnNetworkObjectExample(Vector3 position)
    {
        // Instantiate the prefab on the server
        NetworkObject newObj = Instantiate(spawnablePrefab, position, Quaternion.identity);
        // Move the new object into the same scene as this player (room isolation)
        Scene playerScene = gameObject.scene;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(newObj.gameObject, playerScene);
        // Spawn the object over the network, with ownership given to the caller (the player who clicked)
        base.Spawn(newObj.gameObject, Owner);
    }
}
