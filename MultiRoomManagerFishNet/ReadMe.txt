For testing purposes, set LobbyScene as 0 in your scene inspector, and RoomScene as 1.
Make sure you're also using "old" input system for testing purposes.

This example shows how to make a lobby/room list solution for FishNet, all within the one server under one IP address (no game spawning).
It has everything you need to learn how scene interest management works.

The BasicLobbyExample.cs is a bare bones lobby that works alongside the FishNetNetworkManagerHUD.cs. 
It provides examples of creating a room, selecting scene for the room, maximum player count, room name and extra room data (string variable).
The extra room data could be game mode information or even a JSON string.
Since scenes are loaded Additively, I recommend placing your whole "main menu" scene under this script as it will autodestroy upon connecting to a room.

The BasicPlayerController.cs also provides an example on how to spawn a network object into the player's room.