using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class PlayerMovementManager : MonoBehaviour
{
    [SerializeField]
    private DynamicMoveProvider dynamicMoveProvider;
    [SerializeField]
    CharacterController controller;
    [SerializeField]
    public InputActionProperty thumbstickDown;
    [SerializeField]
    private int walkSpeed;
    [SerializeField]
    private int sprintSpeed;
    private bool isSprinting = false;
    public InputActionProperty thumbstickUp;
    private Vector3 velocity;
    [SerializeField]
    private float jumpSpeed = 2;
    private float defaultGravity;
    public float currentGravity;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultGravity = Physics.gravity.y;
        currentGravity = defaultGravity;
    }

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
        velocity.y += currentGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Reset velocity if grounded
        if (controller.isGrounded && velocity.y< 0)
        {
            velocity.y = 0f;
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

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        thumbstickUp.action.performed += Jump;
    }

    private void OnDisable()
    {
        thumbstickUp.action.performed -= Jump;
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        if (!controller.isGrounded) return;
            velocity.y += Mathf.Sqrt(jumpSpeed * -3f * Physics.gravity.y);
    }

    public void ResetGravity()
    {
        currentGravity = defaultGravity;
    }

    
}
