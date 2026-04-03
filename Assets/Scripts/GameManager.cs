using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject spawnable1;
    NetworkVariable<int> playersInGame = new NetworkVariable<int>(0);
    [SerializeField] private Vector3 player1SpawnPosition;
    [SerializeField] private Vector3 player2SpawnPosition;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        StartCoroutine(SpawnPlayerDelayed(clientId));
    }

    private System.Collections.IEnumerator SpawnPlayerDelayed(ulong clientId)
    {
        yield return null; // espera 1 frame

        GameObject instance = Instantiate(spawnable1, new Vector3(290, 60, 290), Quaternion.identity);
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

        // Fuerza activación de todos los NetworkBehaviours antes del spawn
        foreach (var nb in instance.GetComponentsInChildren<NetworkBehaviour>(true))
        {
            nb.enabled = true;
            if (!nb.gameObject.activeSelf)
                nb.gameObject.SetActive(true);
        }

        instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}