using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class FlagStandController : MonoBehaviour
{
    private XRSocketInteractor xrSocket;

    //Si no es un socket del P1, será del P2
    [SerializeField] bool isP1FlagStand;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        xrSocket = GetComponentInChildren<XRSocketInteractor>();
        xrSocket.selectEntered.AddListener(DebugLogFlagDetected);
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
            }
        }
        else
        {
           if (arg0.interactableObject.transform.CompareTag("P1Flag"))
            {
                Debug.Log("El juego debería acabar. TODO: Pensar en cómo hacer que acabe.");
            } 
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
