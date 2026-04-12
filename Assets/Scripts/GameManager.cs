using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject spawnable1;
    public GameObject spawnable2;
    [SerializeField] private Vector3 player1SpawnPosition;
    [SerializeField] private Vector3 player2SpawnPosition;
    public bool gameStarting = false;

    NetworkVariable<int> playersInGame = new NetworkVariable<int> (0);
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }
    private void Update()
    {
        if (!gameStarting)
            if (playersInGame.Value > 1)
            {
                gameStarting = true;
            }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Instantiate on the server
        playersInGame.Value++;
        Vector3 playerSpawnPosition = Vector3.zero;
        GameObject spawnedObject;
        if (clientId == 0)
        {
            playerSpawnPosition = player1SpawnPosition;
            spawnedObject = Instantiate(spawnable1, playerSpawnPosition, Quaternion.identity);
        }
        else
        {
            playerSpawnPosition = player2SpawnPosition;
            spawnedObject = Instantiate(spawnable2, playerSpawnPosition, Quaternion.identity);
        }

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

