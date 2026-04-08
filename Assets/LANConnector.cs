using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro; // If using TextMeshPro for the input field

public class LANConnector : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_InputField ipInputField; // Drag your UI Input Field here

    // Call this from your "Start Host" button
    public void StartHostGame()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // This is the magic line. 
        // We tell the Host: target yourself (127.0.0.1), keep the current port, 
        // but OPEN the listen address to the whole network (0.0.0.0)
        transport.SetConnectionData("127.0.0.1", transport.ConnectionData.Port, "0.0.0.0");

        NetworkManager.Singleton.StartHost();
    }

    public void StartClientGame()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Purge invisible Android characters and trim spaces
        string targetIP = ipInputField.text.Replace("\u200B", "").Trim();
        if (string.IsNullOrEmpty(targetIP)) targetIP = "127.0.0.1";

        // We tell the Client: connect to the exact IP the user typed, using the current port
        transport.SetConnectionData(targetIP, transport.ConnectionData.Port);

        NetworkManager.Singleton.StartClient();
    }
}