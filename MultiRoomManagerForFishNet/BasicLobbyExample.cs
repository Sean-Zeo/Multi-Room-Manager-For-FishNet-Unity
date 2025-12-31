using UnityEngine;
using System.Collections.Generic;
using FishNet;
using FishNet.Transporting;
public class BasicLobbyExample : MonoBehaviour
{
    string nameField = "Room";
    string dataField = "";
    string sceneField = "RoomScene";
    string maxField = "12";

    const int panelWidth = 340;
    const int marginRight = 10;

    void OnEnable()
    {
        nameField = "Room " + Random.Range(100, 999).ToString();
    }

    void Start()
    {
        InstanceFinder.ClientManager.RegisterBroadcast<RoomListResponseMessage>(OnRoomList);
    }

    void OnDestroy()
    {
        if(InstanceFinder.ClientManager != null)
        InstanceFinder.ClientManager.UnregisterBroadcast<RoomListResponseMessage>(OnRoomList);
    }

    void OnGUI()
    {
        if (!InstanceFinder.IsClientStarted)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 20));
            GUILayout.Label("Not connected");
            GUILayout.EndArea();
            return;
        }

        int baseX = Screen.width - panelWidth - marginRight;
        GUILayout.BeginArea(new Rect(baseX, 10, panelWidth, Screen.height - 10));
        GUILayout.Label("Create Room", GUILayout.Height(20));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Room Name", GUILayout.Width(120));
        nameField = GUILayout.TextField(nameField, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Room Data", GUILayout.Width(120));
        dataField = GUILayout.TextField(dataField, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Room Scene", GUILayout.Width(120));
        sceneField = GUILayout.TextField(sceneField, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Players", GUILayout.Width(120));
        maxField = GUILayout.TextField(maxField, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Room", GUILayout.Width(100)))
        {
            if (int.TryParse(maxField, out int m))
            {
                var msg = new CreateRoomMessage
                {
                    roomName = nameField,
                    roomData = dataField,
                    sceneName = sceneField,
                    maxPlayers = m
                };

                InstanceFinder.ClientManager.Broadcast(msg);
            }
            Destroy(this.gameObject);
        }

        if (GUILayout.Button("Refresh Room List", GUILayout.Width(150)))
        {
            InstanceFinder.ClientManager.Broadcast(new RoomListRequestMessage());
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Room List", GUILayout.Height(20));
        foreach (var e in rooms)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Join {e.name} ({e.cur}/{e.max})", GUILayout.Width(200)))
            {
                InstanceFinder.ClientManager.Broadcast(new JoinRoomMessage { roomName = e.name });
                Destroy(this.gameObject);
            }

            GUILayout.Label("Data: " + e.data);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        GUILayout.EndArea();
    }

    struct Entry { public string name, data, scene; public int cur, max; }
    List<Entry> rooms = new List<Entry>();
    void OnRoomList(RoomListResponseMessage msg, Channel channel)
    {
        rooms.Clear();
        if (msg.roomNames == null) return;

        for (int i = 0; i < msg.roomNames.Length; i++)
        {
            rooms.Add(new Entry
            {
                name = msg.roomNames[i],
                data = msg.roomDatas[i],
                scene = msg.sceneNames[i],
                cur = msg.currentCounts[i],
                max = msg.maxCounts[i]
            });
        }
    }
}