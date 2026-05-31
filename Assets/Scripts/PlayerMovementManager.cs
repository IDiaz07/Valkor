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
    public float sprintStaminaConsumption;
    private float jumpSpeed = 2;
    private float defaultGravity;
    public float currentGravity;
    private Vector2 horizontalVelocity;

    [SerializeField]
    private CharacterLife playerStats;

    // ── AUDIO ──────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioClip sprintStartClip;   // Sonido al activar sprint
    [SerializeField] private AudioClip sprintStopClip;    // Sonido al cancelar sprint
    [SerializeField] private AudioClip jumpClip;          // Sonido al saltar
    [SerializeField] private AudioClip staminaEmptyClip;  // Sonido al quedarse sin stamina
    private AudioSource audioSource;
    private bool staminaEmptyPlayed = false;              // Evita que el sonido suene en bucle
    // ───────────────────────────────────────────────────────

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultGravity = Physics.gravity.y;
        currentGravity = defaultGravity;
        audioSource = GetComponent<AudioSource>();
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
                isSprinting = true;
            }
        }

        horizontalVelocity.x = controller.velocity.x;
        horizontalVelocity.y = controller.velocity.z;

        if (isSprinting && horizontalVelocity.magnitude > 0.1f)
            playerStats.ActualStamina -= sprintStaminaConsumption * Time.deltaTime;

        if (playerStats.ActualStamina <= 0)
        {
            // Suena UNA sola vez cuando la stamina llega a cero
            if (!staminaEmptyPlayed)
            {
                staminaEmptyPlayed = true;
                if (audioSource != null && staminaEmptyClip != null)
                    audioSource.PlayOneShot(staminaEmptyClip);
            }
            isSprinting = false;
            CancelSprint();
        }
        else
        {
            // Resetear el flag cuando la stamina se recupera
            staminaEmptyPlayed = false;
        }

        velocity.y += currentGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Reset velocity if grounded
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
    }

    private void InitiateSprint()
    {
        dynamicMoveProvider.moveSpeed = sprintSpeed;
        Debug.Log("Sprinting");

        if (audioSource != null && sprintStartClip != null)
            audioSource.PlayOneShot(sprintStartClip);
    }

    private void CancelSprint()
    {
        dynamicMoveProvider.moveSpeed = walkSpeed;
        Debug.Log("Walking");

        if (audioSource != null && sprintStopClip != null)
            audioSource.PlayOneShot(sprintStopClip);
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
        playerStats.ActualStamina -= 10;

        if (audioSource != null && jumpClip != null)
            audioSource.PlayOneShot(jumpClip);
    }

    public void ResetGravity()
    {
        currentGravity = defaultGravity;
    }
}
