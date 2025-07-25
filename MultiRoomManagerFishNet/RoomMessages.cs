using FishNet.Broadcast; 

public struct RoomListRequest : IBroadcast
{
    //No data needed for request 
}

public struct RoomListResponse : IBroadcast
{
    public string[] roomNames;
    public string[] roomDatas;
    public string[] sceneNames;
    public int[] currentCounts;
    public int[] maxCounts;
}

public struct CreateRoomRequest : IBroadcast
{
    public string roomName;
    public string roomData;
    public string sceneName;
    public int maxPlayers;
}

public struct JoinRoomRequest : IBroadcast
{
    public string roomName;
}
