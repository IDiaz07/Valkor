using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class FlagStandController : MonoBehaviour
{
    private XRSocketInteractor xrSocket;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        xrSocket = GetComponentInChildren<XRSocketInteractor>();
        xrSocket.selectEntered.AddListener(DebugLogFlagDetected);
    }

    private void DebugLogFlagDetected(SelectEnterEventArgs arg0)
    {
        //TODO El comportamiento que decidamos que ocurra cuando se detecta una bandera
        Debug.Log("A Flag ("+arg0.interactableObject.transform.gameObject.name+") has been detected!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
