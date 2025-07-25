using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using FishNet;

public class MultiRoomManager : NetworkBehaviour
{
    public class RoomInfo
    {
        public string roomName;
        public string roomData;
        public string sceneName;
        public int currentPlayers;
        public int maxPlayers;
        public Scene scene;
        public List<NetworkConnection> playerConnections = new List<NetworkConnection>();
    }

    [Header("Physics")]
    public LocalPhysicsMode roomPhysicsMode = LocalPhysicsMode.None;

    [Header("Prefabs")]
    public GameObject lobbyPlayerPrefab;
    public GameObject roomPlayerPrefab;

    [HideInInspector]
    public List<RoomInfo> rooms = new();

    readonly Dictionary<NetworkConnection, RoomInfo> connectionToRoom = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
        rooms.Clear();

        InstanceFinder.ServerManager.RegisterBroadcast<RoomListRequest>(OnRoomListRequest);
        InstanceFinder.ServerManager.RegisterBroadcast<CreateRoomRequest>(OnCreateRoom);
        InstanceFinder.ServerManager.RegisterBroadcast<JoinRoomRequest>(OnJoinRoom);
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        InstanceFinder.ServerManager.UnregisterBroadcast<RoomListRequest>(OnRoomListRequest);
        InstanceFinder.ServerManager.UnregisterBroadcast<CreateRoomRequest>(OnCreateRoom);
        InstanceFinder.ServerManager.UnregisterBroadcast<JoinRoomRequest>(OnJoinRoom);
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Stopped)
            return;

        var oldObj = conn.FirstObject;
        if (oldObj != null)
        {
            Scene playerScene = oldObj.gameObject.scene;
            RoomInfo info = rooms.Find(r => r.scene == playerScene);
            if (info != null)
            {
                info.currentPlayers--;
                info.playerConnections.Remove(conn);
                connectionToRoom.Remove(conn);

                if (info.currentPlayers <= 0)
                    StartCoroutine(UnloadRoomWhenEmpty(info));
            }
        }
    }

    private IEnumerator UnloadRoomWhenEmpty(RoomInfo info)
    {
        yield return UnitySceneManager.UnloadSceneAsync(info.scene);
        rooms.Remove(info);
    }

    private void OnRoomListRequest(NetworkConnection conn, RoomListRequest msg, Channel channel)
    {
        int n = rooms.Count;
        var resp = new RoomListResponse
        {
            roomNames = new string[n],
            roomDatas = new string[n],
            sceneNames = new string[n],
            currentCounts = new int[n],
            maxCounts = new int[n]
        };

        for (int i = 0; i < n; i++)
        {
            var r = rooms[i];
            resp.roomNames[i] = r.roomName;
            resp.roomDatas[i] = r.roomData;
            resp.sceneNames[i] = r.sceneName;
            resp.currentCounts[i] = r.currentPlayers;
            resp.maxCounts[i] = r.maxPlayers;
        }

        conn.Broadcast(resp);
    }

    private void OnCreateRoom(NetworkConnection conn, CreateRoomRequest msg, Channel channel)
    {
        if (connectionToRoom.ContainsKey(conn))
        {
            Debug.LogWarning($"[Server] {conn} already in a room.");
            return;
        }

        if (rooms.Exists(r => r.roomName == msg.roomName))
        {
            Debug.LogWarning($"[Server] Room '{msg.roomName}' already exists.");
            return;
        }

        StartCoroutine(CreateRoomCoroutine(conn, msg));
    }

    private IEnumerator CreateRoomCoroutine(NetworkConnection conn, CreateRoomRequest msg)
    {
        var loadOp = UnitySceneManager.LoadSceneAsync(
            msg.sceneName,
            new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = roomPhysicsMode
            });

        yield return loadOp;

        Scene newScene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);

        var info = new RoomInfo
        {
            roomName = msg.roomName,
            roomData = msg.roomData,
            sceneName = msg.sceneName,
            currentPlayers = 0,
            maxPlayers = msg.maxPlayers,
            scene = newScene
        };
        rooms.Add(info);

        InstanceFinder.SceneManager.LoadConnectionScenes(conn, new SceneLoadData(newScene));
        yield return null;

        var roomGO = Instantiate(roomPlayerPrefab);
        InstanceFinder.ServerManager.Spawn(roomGO, conn);
        UnitySceneManager.MoveGameObjectToScene(roomGO, newScene);

        info.currentPlayers++;
        info.playerConnections.Add(conn);
        connectionToRoom[conn] = info;

        if (conn.FirstObject != null)
            InstanceFinder.ServerManager.Despawn(conn.FirstObject);
    }

    private void OnJoinRoom(NetworkConnection conn, JoinRoomRequest msg, Channel channel)
    {
        if (connectionToRoom.ContainsKey(conn))
            return;

        var info = rooms.Find(r => r.roomName == msg.roomName);
        if (info == null || info.currentPlayers >= info.maxPlayers)
            return;

        StartCoroutine(JoinRoomCoroutine(conn, info));
    }

    private IEnumerator JoinRoomCoroutine(NetworkConnection conn, RoomInfo info)
    {
        InstanceFinder.SceneManager.LoadConnectionScenes(conn, new SceneLoadData(info.scene));
        yield return null;

        var roomGO = Instantiate(roomPlayerPrefab);
        InstanceFinder.ServerManager.Spawn(roomGO, conn);
        UnitySceneManager.MoveGameObjectToScene(roomGO, info.scene);

        info.currentPlayers++;
        info.playerConnections.Add(conn);
        connectionToRoom[conn] = info;

        if (conn.FirstObject != null)
            InstanceFinder.ServerManager.Despawn(conn.FirstObject);
    }

    public RoomInfo GetRoomInfoFromScene(Scene scene)
    {
        return rooms.Find(r => r.scene.handle == scene.handle);
    }
}
