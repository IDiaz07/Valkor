using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject spawnable1;
    public GameObject spawnable2;

    NetworkVariable<int> playersInGame = new NetworkVariable<int> (0);
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                Debug.Log("Connected: " + clientId);
                GameObject spawnedObject = Instantiate(spawnable1);
                spawnedObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            };
            NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
            {
                Debug.Log("Disconnected: " + clientId);
            };
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                Debug.Log("Connected: " + clientId);
                GameObject spawnedObject = Instantiate(spawnable2);
                spawnedObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            };
            NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
            {
                Debug.Log("Disconnected: " + clientId);
            };
        }

            base.OnNetworkSpawn();
    }

   
}
