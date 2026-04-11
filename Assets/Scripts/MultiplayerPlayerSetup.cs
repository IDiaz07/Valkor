using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
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
            // --- THIS IS YOU (THE HOST/LOCAL PLAYER) ---

            // 1. Claim the Main Camera tag locally
            if (playerCamera != null) playerCamera.gameObject.tag = "MainCamera";

            // 2. Enable local inputs safely
            InputActionManager inputManager = GetComponentInChildren<InputActionManager>(true);
            if (inputManager != null) inputManager.enabled = true;

            // 3. Enable Camera and Audio
            playerCamera.enabled = true;
            playerListener.enabled = true;
            if (Camera.main != null && Camera.main != playerCamera)
            {
                Camera.main.gameObject.SetActive(false);
            }


            // 4. WAKE UP your Custom Movement Script
            var jumpScript = GetComponentInChildren<PlayerMovementManager>(true);
            if (jumpScript != null) jumpScript.enabled = true;

            XRInputModalityManager xRInputModalityManager = GetComponentInChildren<XRInputModalityManager>(true);
            if (xRInputModalityManager != null) xRInputModalityManager.enabled = true;

            // 5. WAKE UP the Body Transformer
            var bodyTransformer = GetComponentInChildren<XRBodyTransformer>(true);
            if (bodyTransformer != null) bodyTransformer.enabled = true;
        }
        else
        {
            // --- THIS IS A REMOTE PLAYER (CLIENT) ---

            playerCamera.enabled = false;
            playerListener.enabled = false;

            // Untag the remote camera so it doesn't break physics
            if (playerCamera != null) playerCamera.gameObject.tag = "Untagged";

            XROrigin xrOrigin = GetComponent<XROrigin>();
            if (xrOrigin != null) xrOrigin.enabled = false;

            CharacterController charController = GetComponent<CharacterController>();
            if (charController != null) charController.enabled = false;

            // NEW: Kill the Character Controller Driver
            CharacterControllerDriver ccDriver = GetComponentInChildren<CharacterControllerDriver>();
            if (ccDriver != null) ccDriver.enabled = false;

            LocomotionMediator locoMediator = GetComponentInChildren<LocomotionMediator>();
            if (locoMediator != null) locoMediator.enabled = false;

            // FIX: Explicitly KILL the built-in locomotion providers
            var locomotionProviders = GetComponentsInChildren<LocomotionProvider>();
            foreach (var provider in locomotionProviders) provider.enabled = false;


            var jumpScript = GetComponentInChildren<PlayerMovementManager>();
            if (jumpScript != null) jumpScript.enabled = false;

            Transform localManagers = transform.Find("LocalManagers");
            if (localManagers != null)
            {
                InputActionManager inputManager = localManagers.GetComponentInChildren<InputActionManager>();
                if (inputManager != null && inputManager.actionAssets != null)
                {
                    inputManager.actionAssets.Clear();
                }
                Destroy(localManagers.gameObject);
            }

            TrackedPoseDriver[] poseDrivers = GetComponentsInChildren<TrackedPoseDriver>();
            foreach (var driver in poseDrivers) driver.enabled = false;

            XRInteractionGroup[] xrGroups = GetComponentsInChildren<XRInteractionGroup>();
            foreach (var group in xrGroups) group.enabled = false;

            XRBaseInteractor[] interactors = GetComponentsInChildren<XRBaseInteractor>();
            foreach (var interactor in interactors) interactor.enabled = false;

            HandAnimator[] handAnimators = GetComponentsInChildren<HandAnimator>();
            foreach (HandAnimator handAnimator in handAnimators) handAnimator.enabled = false;

            XRInputModalityManager xRInputModalityManager = GetComponentInChildren<XRInputModalityManager>(true);
            if (xRInputModalityManager != null) xRInputModalityManager.enabled = false;

            // KILL the XRBodyTransformer so it doesn't hijack your locomotion
            var bodyTransformer = GetComponentInChildren<XRBodyTransformer>();
            if (bodyTransformer != null) bodyTransformer.enabled = false;
        }
    }
}