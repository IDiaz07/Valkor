using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    private CharacterController controller;
    public InputActionProperty thumbstickUp;
    private Vector3 velocity;

    // New flag to track climbing
    [HideInInspector]
    public bool isClimbing = false;

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

    }

    void Update()
    {

        // Only apply gravity if not climbing
        if (!isClimbing)
        {
            
            velocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        else { velocity.y = 0f; 
        }
        // Reset velocity if grounded
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
    }
       
    }

