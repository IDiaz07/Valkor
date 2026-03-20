using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
            // 1. Find and disable all TrackedPoseDrivers (Head and Hands tracking)
            TrackedPoseDriver[] poseDrivers = GetComponentsInChildren<TrackedPoseDriver>();
            foreach (var driver in poseDrivers)
            {
                driver.enabled = false;
            }

            // 2. Find and disable all XR Controllers (Interaction tracking for Hands)
            XRInteractionGroup[] xrControllers = GetComponentsInChildren<XRInteractionGroup>();
            foreach (var controller in xrControllers)
            {
                controller.enabled = false;
            }
        }
    }
}
