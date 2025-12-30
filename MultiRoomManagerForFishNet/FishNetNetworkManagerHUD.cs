using UnityEngine;
using FishNet;
using FishNet.Transporting;

public class FishNetNetworkManagerHUD : MonoBehaviour
{
    public int offsetX;
    public int offsetY;
    private string addressInput = "localhost";
    private string portInput = "7777";

    public HeadlessStartMode serverBuildStartMode = HeadlessStartMode.None;
    public enum HeadlessStartMode { None, AutoStartServer, AutoStartClient };

    private void Start()
    {
        #if UNITY_SERVER
        if (serverBuildStartMode == HeadlessStartMode.AutoStartServer)
            InstanceFinder.ServerManager.StartConnection();
        else if(serverBuildStartMode == HeadlessStartMode.AutoStartClient) 
            InstanceFinder.ClientManager.StartConnection();
        #endif
    }

    private void OnGUI()
    {
        LocalConnectionState clientState = InstanceFinder.TransportManager.Transport.GetConnectionState(false);
        LocalConnectionState serverState = InstanceFinder.TransportManager.Transport.GetConnectionState(true);

        int width = 300;
        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, width, 9999));
        if (clientState == LocalConnectionState.Stopped && serverState == LocalConnectionState.Stopped) {
            #if !UNITY_WEBGL
            if (GUILayout.Button("Host (Server + Client)"))
            {
                InstanceFinder.NetworkManager.TransportManager.Transport.SetClientAddress("localhost");
                InstanceFinder.NetworkManager.TransportManager.Transport.SetPort(ushort.Parse(portInput));
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();
            }
            #endif
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Client"))
            {
                InstanceFinder.NetworkManager.TransportManager.Transport.SetClientAddress(addressInput);
                InstanceFinder.NetworkManager.TransportManager.Transport.SetPort(ushort.Parse(portInput));
                InstanceFinder.ClientManager.StartConnection();
            }
            addressInput = GUILayout.TextField(addressInput);
            portInput = GUILayout.TextField(portInput);
            GUILayout.EndHorizontal();

            #if !UNITY_WEBGL
            if (GUILayout.Button("Server Only"))
            {
                InstanceFinder.NetworkManager.TransportManager.Transport.SetPort(ushort.Parse(portInput));
                InstanceFinder.ServerManager.StartConnection();
            }
            #endif
        }
        else
        {
            if (clientState != LocalConnectionState.Stopped)
            {
                if (clientState == LocalConnectionState.Starting)
                {
                    GUILayout.Label($"Connecting to " + addressInput + "..");
                }
                else if(clientState == LocalConnectionState.Started)
                {
                    GUILayout.Label($"<b>Client</b>: connected to {addressInput}");
                }

                if (serverState == LocalConnectionState.Started)
                {
                    if (GUILayout.Button("Stop Server + Client"))
                        InstanceFinder.TransportManager.Transport.StopConnection(true);
                }
                else
                {
                    if (GUILayout.Button("Stop Client"))
                        InstanceFinder.TransportManager.Transport.StopConnection(false);
                }
            }

            if(serverState != LocalConnectionState.Stopped && clientState == LocalConnectionState.Stopped)
            {
                if(serverState == LocalConnectionState.Starting)
                {
                    GUILayout.Label($"Starting server..");
                }else if(serverState == LocalConnectionState.Started)
                {
                    GUILayout.Label("<b>Server</b>: running");
                    if (GUILayout.Button("Stop Server"))
                        InstanceFinder.TransportManager.Transport.StopConnection(true);
                }
            }
        }
        GUILayout.EndArea();
    }
}
