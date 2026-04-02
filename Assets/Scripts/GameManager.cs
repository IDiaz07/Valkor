using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject spawnable1;
    NetworkVariable<int> playersInGame = new NetworkVariable<int>(0);

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