using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    // Este metodo se ejecuta en el server, a petición del cliente
    [Rpc(SendTo.Server, InvokePermission =RpcInvokePermission.Everyone)]
    private void RequestOwnershipServerRpc(RpcParams rpcParams = default)
    {
        // The server transfers ownership to the client who sent the request
        netObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }
    }
}