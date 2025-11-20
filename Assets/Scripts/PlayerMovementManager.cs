using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlayerMovementManager : MonoBehaviour
{
    [SerializeField]
    private DynamicMoveProvider dynamicMoveProvider;
    [SerializeField]
    public InputActionProperty thumbstickDown;
    [SerializeField]
    private int walkSpeed;
    [SerializeField]
    private int sprintSpeed;
    private bool isSprinting = false;


    // Update is called once per frame
    void Update()
    {
        if (thumbstickDown.action.WasPressedThisFrame())
        {
            if (isSprinting)
            {
                isSprinting = false;
                CancelSprint();
            }
            else
            {
                InitiateSprint();
                isSprinting=true;
            }
        }
    }

    private void InitiateSprint()
    {
        dynamicMoveProvider.moveSpeed = sprintSpeed;
        Debug.Log("Sprinting");
    }

    private void CancelSprint()
    {
        dynamicMoveProvider.moveSpeed = walkSpeed;
        Debug.Log("Walking");
    }
}
