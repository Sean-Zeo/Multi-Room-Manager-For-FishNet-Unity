using FishNet.Broadcast;
public struct RoomListRequestMessage : IBroadcast { }
public struct RoomListResponseMessage : IBroadcast
{
    public string[] roomNames;
    public string[] roomDatas;
    public string[] sceneNames;
    public int[] currentCounts;
    public int[] maxCounts;
}

public struct CreateRoomMessage : IBroadcast
{
    public string roomName;
    public string roomData;
    public string sceneName;
    public int maxPlayers;
}

public struct JoinRoomMessage : IBroadcast
{
    public string roomName;
}
