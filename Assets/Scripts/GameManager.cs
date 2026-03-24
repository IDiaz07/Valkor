using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject spawnable1;
    [SerializeField] private Vector3 player1SpawnPosition;
    [SerializeField] private Vector3 player2SpawnPosition;

    NetworkVariable<int> playersInGame = new NetworkVariable<int> (0);
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Instantiate on the server
        Vector3 playerSpawnPosition = Vector3.zero;
        if (clientId == NetworkManager.LocalClientId)
        {
            playerSpawnPosition = player1SpawnPosition;
        }
        else
        {
            playerSpawnPosition = player2SpawnPosition;
        }
        GameObject spawnedObject = Instantiate(spawnable1, playerSpawnPosition , Quaternion.identity);

        // Spawn it across the network and assign ownership to the connected client
        spawnedObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}

