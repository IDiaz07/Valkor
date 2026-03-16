using Unity.Netcode;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    public void StartHost()
    {
        Destroy(player);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        Destroy(player);
        NetworkManager.Singleton.StartClient();
    }

}
