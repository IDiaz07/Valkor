using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FlagStandController : MonoBehaviour
{
    private XRSocketInteractor xrSocket;
    private NetworkManager networkManager;

    // Si no es un socket del P1, será del P2
    [SerializeField] bool isP1FlagStand;

    // ── AUDIO ──────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioClip flagCapturedClip;  // Sonido al capturar la bandera (victoria)
    private AudioSource audioSource;
    // ───────────────────────────────────────────────────────

    void Awake()
    {
        xrSocket = GetComponentInChildren<XRSocketInteractor>();
        xrSocket.selectEntered.AddListener(DebugLogFlagDetected);
        networkManager = FindAnyObjectByType<NetworkManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void DebugLogFlagDetected(SelectEnterEventArgs arg0)
    {
        Debug.Log("A Flag (" + arg0.interactableObject.transform.gameObject.name + ") has been detected!");

        if (isP1FlagStand)
        {
            if (arg0.interactableObject.transform.CompareTag("P2Flag"))
            {
                Debug.Log("El juego debería acabar.");

                // Reproducir sonido de captura localmente (lo oirá el jugador que esté cerca)
                if (audioSource != null && flagCapturedClip != null)
                    audioSource.PlayOneShot(flagCapturedClip);

                if (!networkManager.IsServer) return;

                NetworkObject netObj = arg0.interactableObject.transform.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                {
                    netObj.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
                }

                MatchEndHandler matchHandler = FindAnyObjectByType<MatchEndHandler>();
                if (matchHandler != null)
                {
                    matchHandler.TriggerGameOverRpc(0);
                }
            }
        }
        else
        {
            if (arg0.interactableObject.transform.CompareTag("P1Flag"))
            {
                Debug.Log("El juego debería acabar.");

                // Reproducir sonido de captura localmente
                if (audioSource != null && flagCapturedClip != null)
                    audioSource.PlayOneShot(flagCapturedClip);

                if (!networkManager.IsServer) return;

                Debug.Log("Is server");

                MatchEndHandler matchHandler = FindAnyObjectByType<MatchEndHandler>();
                if (matchHandler != null)
                {
                    Debug.Log("¡Casi hemos llegado a matchHandler!");
                    matchHandler.TriggerGameOverRpc(1);
                }
            }
        }
    }

    void Update()
    {

    }
}
