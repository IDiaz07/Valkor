using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class FlagStandController : MonoBehaviour
{
    private XRSocketInteractor xrSocket;
    private NetworkManager networkManager;

    //Si no es un socket del P1, será del P2
    [SerializeField] bool isP1FlagStand;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        xrSocket = GetComponentInChildren<XRSocketInteractor>();
        xrSocket.selectEntered.AddListener(DebugLogFlagDetected);
        networkManager = FindAnyObjectByType<NetworkManager>();
    }

    private void DebugLogFlagDetected(SelectEnterEventArgs arg0)
    {
        //TODO El comportamiento que decidamos que ocurra cuando se detecta una bandera
        Debug.Log("A Flag (" + arg0.interactableObject.transform.gameObject.name + ") has been detected!");
        if (isP1FlagStand)
        {
            if (arg0.interactableObject.transform.CompareTag("P2Flag"))
            {
                Debug.Log("El juego debería acabar. TODO: Pensar en cómo hacer que acabe.");
                if (!networkManager.IsServer) return;
                NetworkObject netObj = arg0.interactableObject.transform.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                {
                    // The server reclaims the object
                    netObj.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
                }
                MatchEndHandler matchHandler = FindAnyObjectByType<MatchEndHandler>();
                if (matchHandler != null)
                {
                    // Broadcast the winning ID to all players
                    matchHandler.TriggerGameOverRpc(0);
                }
            }
        }
        else
        {
            if (arg0.interactableObject.transform.CompareTag("P1Flag"))
            {
                Debug.Log("El juego debería acabar. TODO: Pensar en cómo hacer que acabe.");
                if (!networkManager.IsServer) return;
                Debug.Log("Is server");
                MatchEndHandler matchHandler = FindAnyObjectByType<MatchEndHandler>();
                if (matchHandler != null)
                {
                    Debug.Log("¡Casi hemos llegado a matchHandler!");
                    // Broadcast the winning ID to all players
                    matchHandler.TriggerGameOverRpc(1);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
