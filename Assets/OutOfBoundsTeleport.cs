using Unity.Netcode;
using UnityEngine;

public class OutOfBoundsTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Las coordenadas del teletransporte")]
    public Vector3 respawnPosition = new Vector3(0, 2f, 0);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponentInParent<NetworkObject>();

            //Solo el jugador dueño del player es capaz de teletransportarlo
            if (netObj != null && netObj.IsOwner)
            {
                CharacterController charController = netObj.GetComponent<CharacterController>();

                if (charController != null)
                {
                    // Se desactiva y se activa para evitar que interfiera con el teletransporte
                    charController.enabled = false;
                    netObj.transform.position = respawnPosition;
                    charController.enabled = true;
                }
                else
                {
                    netObj.transform.position = respawnPosition;
                }
            }
        }
    }
}