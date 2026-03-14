using Unity.Netcode;
using UnityEngine;

public class MultiplayerPlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerListener;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // The player you control
            playerCamera.enabled = true;
            playerListener.enabled = true;

            // If you have a main menu camera, disable it now
            if (Camera.main != null && Camera.main != playerCamera)
            {
                Camera.main.gameObject.SetActive(false);
            }
        }
        else
        {
            // This is ANOTHER player..
            playerCamera.enabled = false;
            playerListener.enabled = false;
        }
    }
}
