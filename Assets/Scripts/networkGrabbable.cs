using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable), typeof(NetworkObject))]
public class NetworkGrabbable : NetworkBehaviour
{
    private XRGrabInteractable grabInteractable;
    private NetworkObject netObject;

    // ── AUDIO ──────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioClip grabClip;   // Sonido al agarrar este objeto
    private AudioSource audioSource;
    // ───────────────────────────────────────────────────────

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        netObject = GetComponent<NetworkObject>();
        audioSource = GetComponent<AudioSource>();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Reproducir sonido localmente cuando este jugador agarra el objeto
        if (audioSource != null && grabClip != null)
            audioSource.PlayOneShot(grabClip);

        if (netObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            // Si soy un cliente y no soy owner de este objeto, se lo pido al servidor
            if (!IsServer)
            {
                RequestOwnershipServerRpc();
            }
            else
            {
                netObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    /// Este metodo se ejecuta en el server, a petición del cliente
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestOwnershipServerRpc(RpcParams rpcParams = default)
    {
        // Comprobar que hay interactores antes de acceder
        if (grabInteractable.interactorsSelecting.Count > 0)
        {
            var currentInteractor = grabInteractable.interactorsSelecting[0];

            // Si el interactor es un socket
            if (currentInteractor is XRSocketInteractor socket)
            {
                grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);

                // Desactivar temporalmente el socket
                StartCoroutine(TemporarilyDisableSocket(socket));
            }
        }
        else
        {
            Debug.LogWarning("RequestOwnershipServerRpc: No interactors selecting this object on server.");
        }

        // Transferir ownership al cliente que hizo la petición
        netObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    }

    private IEnumerator TemporarilyDisableSocket(XRSocketInteractor socket)
    {
        socket.socketActive = false;

        // Wait 1 second to let the client physically move the flag out of the socket's trigger zone
        yield return new WaitForSeconds(1f);

        socket.socketActive = true;
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }
    }
}
