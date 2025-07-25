using UnityEngine;
using FishNet;
using FishNet.Managing;
using System.Collections;

//Simple FishNet Network Manager HUD, inspired by Mirror's NetworkManagerHUD.cs
public class FishNetNetworkManagerHUD : MonoBehaviour
{
    private string ipAddress = "localhost";
    private int port = 7770;

    private void Start()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("[FishNetHUD] Headless mode detected - starting as Server Only");
            StartCoroutine(StartServerAfterAFewSeconds());
        }
    }

    IEnumerator StartServerAfterAFewSeconds()
    {
        yield return new WaitForSeconds(2);
        InstanceFinder.ServerManager.StartConnection();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 250), GUI.skin.box);
        GUILayout.Label("<b><size=16>FishNet Network HUD</size></b>");

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("IP:", GUILayout.Width(50));
        ipAddress = GUILayout.TextField(ipAddress, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (!InstanceFinder.IsClient && !InstanceFinder.IsServer)
        {
            if (GUILayout.Button("Start Host", GUILayout.Height(30)))
            {
                InstanceFinder.NetworkManager.TransportManager.Transport.SetClientAddress(ipAddress);
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Start Client", GUILayout.Height(30)))
            {
                InstanceFinder.NetworkManager.TransportManager.Transport.SetClientAddress(ipAddress);
                InstanceFinder.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Start Server Only", GUILayout.Height(30)))
            {
                InstanceFinder.ServerManager.StartConnection();
            }
        }
        else
        {
            GUILayout.Label($"Status: {(InstanceFinder.IsHost ? "Host" : InstanceFinder.IsServer ? "Server" : "Client")}");
            if (GUILayout.Button("Disconnect", GUILayout.Height(30)))
            {
                InstanceFinder.ServerManager.StopConnection(true);
                InstanceFinder.ClientManager.StopConnection();
                //Hacky way to reset
                Application.LoadLevel(0);
            }
        }

        GUILayout.EndArea();
    }
}
