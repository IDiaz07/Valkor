using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

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
                // 1. Disable Tracking
                TrackedPoseDriver[] poseDrivers = GetComponentsInChildren<TrackedPoseDriver>();
                foreach (var driver in poseDrivers)
                {
                    driver.enabled = false;
                }

                // 2. Disable Controller Logic
                XRInteractionGroup[] xrControllers = GetComponentsInChildren<XRInteractionGroup>();
                foreach (var controller in xrControllers)
                {
                    controller.enabled = false;
                }

                // 3. Disable Interactors (Rays, Direct grabs, etc.)
                XRBaseInteractor[] interactors = GetComponentsInChildren<XRBaseInteractor>();
                foreach (var interactor in interactors)
                {
                    interactor.enabled = false;
                }

                // 4. Disable the locomotion provider
                var locomotionProviders = GetComponentsInChildren<LocomotionProvider>();
                foreach (var provider in locomotionProviders)
                {
                    provider.enabled = false;
                }

                // 5. Disable hand animation scripts
                HandAnimator[] handAnimators = GetComponentsInChildren<HandAnimator>();
                foreach(HandAnimator handAnimator in handAnimators)
                {
                    handAnimator.enabled = false;
                }
        }
    }
}
