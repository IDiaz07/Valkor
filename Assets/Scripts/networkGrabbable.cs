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

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        netObject = GetComponent<NetworkObject>();

        // Subscribe to the grab event
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {

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

    // Este metodo se ejecuta en el server, a petici�n del cliente
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestOwnershipServerRpc(RpcParams rpcParams = default)
    {
        {
            var currentInteractor = grabInteractable.interactorsSelecting[0];

            // If the interactor holding it is a socket
            if (currentInteractor is XRSocketInteractor socket)
            {
                // Force the manager to break the connection
                grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);

                // Disable the socket temporarily so it doesn't instantly snap it back
                StartCoroutine(TemporarilyDisableSocket(socket));
            }
        }
        // The server transfers ownership to the client who sent the request
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