# FishNet Multi‑Room Manager

A lightweight, single-process multi-room solution for FishNet that allows you to host **multiple isolated game rooms** on one server using Unity’s additive scenes and FishNet's built in scene-based visibility.
Players connect to a central lobby, where they can view active rooms, create new ones (with a custom name, data string, scene name, and max player count), and join existing rooms. Each room runs in its own additive scene with completely isolated network traffic (spawns, RPCs, syncs).

Think of this as a server-authoritative, open-source alternative to Photon — with no cloud lock-in, and full control of your architecture.
This solution is built to be clean, fast, and easy to understand — following the KISS principle (Keep It Simple,

**Features**  
- **Scene-isolated network rooms** via FishNet’s built-in `SceneCondition`
- **Dynamic room creation** (scene loading + player spawning) from lobby
- **Automatic room cleanup** (unloads scene when empty)
- **Built-in lobby system** (using Unity's `OnGUI`) to create/join rooms
- **LobbyPlayer ↔ RoomPlayer prefab swapping** on room entry
- **Physics-isolated scenes** using Unity’s `LocalPhysicsMode`
- **Scene visibility enforced**: players in Room A cannot see Room B
- **One server, one port, multiple isolated rooms**

**Basic Example Setup Instructions**
1. Assign LobbyScene as scene 0 and RoomScene as scene 1 in the scene build settings
2. Make sure you're using Unity's old Input in build settings
3. Add both prefabs to FishNet’s **Spawnable Prefabs list**
4. Run the LobbyScene


![Example](images/thumbnail.jpg)