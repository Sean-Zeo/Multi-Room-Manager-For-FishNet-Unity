using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiRoomNetworkManager : MonoBehaviour
{
    public static MultiRoomNetworkManager Instance;
    [Tooltip("Optional")]
    public NetworkObject networkLobbyPlayer;
    public LocalPhysicsMode roomPhysicsMode = LocalPhysicsMode.None;

    [HideInInspector]
    public List<RoomInfo> rooms = new List<RoomInfo>();
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
    private readonly Dictionary<NetworkConnection, RoomInfo> connectionToRoom = new();
    bool creatingRoom = false;
    public List<CreateRoomRequest> createRoomRequestQueue = new List<CreateRoomRequest>();
    public class CreateRoomRequest
    {
        public NetworkConnection conn;
        public CreateRoomMessage msg;
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;

        InstanceFinder.ServerManager.RegisterBroadcast<RoomListRequestMessage>(OnRoomListRequest);
        InstanceFinder.ServerManager.RegisterBroadcast<CreateRoomMessage>(OnCreateRoom);
        InstanceFinder.ServerManager.RegisterBroadcast<JoinRoomMessage>(OnJoinRoom);
        InstanceFinder.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        InstanceFinder.ServerManager.UnregisterBroadcast<RoomListRequestMessage>(OnRoomListRequest);
        InstanceFinder.ServerManager.UnregisterBroadcast<CreateRoomMessage>(OnCreateRoom);
        InstanceFinder.ServerManager.UnregisterBroadcast<JoinRoomMessage>(OnJoinRoom);
        InstanceFinder.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs state)
    {
        if(state.ConnectionState == RemoteConnectionState.Started)
        {
            if(networkLobbyPlayer != null)
            {
                NetworkObject newLobbyPlayer = Instantiate(networkLobbyPlayer);
                InstanceFinder.ServerManager.Spawn(newLobbyPlayer, conn);
            }
        }
        else
        {
            if (connectionToRoom.ContainsKey(conn))
            {
                RoomInfo info = connectionToRoom[conn];
                info.currentPlayers--;
                info.playerConnections.Remove(conn);
                connectionToRoom.Remove(conn);

                if (info.currentPlayers <= 0)
                {
                    StartCoroutine(UnloadEmptyScene(info.scene));
                    rooms.Remove(info);
                }
            }
        }
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopped)
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
            Destroy(this.gameObject);
        }
    }

    public void Update()
    {
        if (createRoomRequestQueue.Count > 0)
        {
            if (!creatingRoom)
            {
                if (createRoomRequestQueue[0].conn != null && InstanceFinder.ServerManager.Clients.ContainsKey(createRoomRequestQueue[0].conn.ClientId))
                    StartCoroutine(CreateRoomCoroutine(createRoomRequestQueue[0].conn, createRoomRequestQueue[0].msg));
                createRoomRequestQueue.RemoveAt(0);
            }
        }
    }

    private void OnRoomListRequest(NetworkConnection conn, RoomListRequestMessage msg, Channel channel)
    {
        int n = rooms.Count;
        var resp = new RoomListResponseMessage
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

    private void OnCreateRoom(NetworkConnection conn, CreateRoomMessage msg, Channel channel)
    {
        if (connectionToRoom.ContainsKey(conn))
        {
            Debug.LogWarning($"[Server] {conn} already in room; create ignored.");
            return;
        }

        if (rooms.Exists(r => r.roomName == msg.roomName))
        {
            Debug.LogWarning($"[Server] Room '{msg.roomName}' already exists; ignoring.");
            return;
        }

        CreateRoomRequest newRequest = new CreateRoomRequest();
        newRequest.conn = conn;
        newRequest.msg = msg;
        createRoomRequestQueue.Add(newRequest);
    }

    private void OnJoinRoom(NetworkConnection conn, JoinRoomMessage msg, Channel channel)
    {
        if (connectionToRoom.ContainsKey(conn)) return;

        var info = rooms.Find(r => r.roomName == msg.roomName);
        if (info == null || info.currentPlayers >= info.maxPlayers) return;

        if (conn.FirstObject != null)
            InstanceFinder.ServerManager.Despawn(conn.FirstObject);
        FishNet.Managing.Scened.SceneLoadData sceneLoadData = new FishNet.Managing.Scened.SceneLoadData(info.scene);
        sceneLoadData.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.None;
        InstanceFinder.SceneManager.LoadConnectionScenes(conn, sceneLoadData);

        connectionToRoom[conn] = info;
        info.currentPlayers++;
        info.playerConnections.Add(conn);
    }

    IEnumerator CreateRoomCoroutine(NetworkConnection conn, CreateRoomMessage msg)
    {
        creatingRoom = true;
        LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive, roomPhysicsMode);
        var loadOp = SceneManager.LoadSceneAsync(msg.sceneName, parameters);
        while (!loadOp.isDone) 
            yield return null;

        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        if (conn != null && InstanceFinder.ServerManager.Clients.ContainsKey(conn.ClientId))
        {
            var info = new RoomInfo
            {
                roomName = msg.roomName,
                roomData = msg.roomData,
                sceneName = msg.sceneName,
                currentPlayers = 0,
                maxPlayers = msg.maxPlayers,
                scene = newScene
            };
            if (conn.FirstObject != null)
                InstanceFinder.ServerManager.Despawn(conn.FirstObject);
            FishNet.Managing.Scened.SceneLoadData sceneLoadData = new FishNet.Managing.Scened.SceneLoadData(info.scene);
            sceneLoadData.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.None;
            InstanceFinder.SceneManager.LoadConnectionScenes(conn, sceneLoadData);

            connectionToRoom[conn] = info;
            info.currentPlayers++;
            info.playerConnections.Add(conn);
            rooms.Add(info);
        }
        else
        {
            StartCoroutine(UnloadEmptyScene(newScene));
        }
        creatingRoom = false;
    }

    IEnumerator UnloadEmptyScene(Scene scene)
    {
        yield return SceneManager.UnloadSceneAsync(scene);
    }
}
