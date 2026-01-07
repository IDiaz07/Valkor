using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class CiclopeMovement : MonoBehaviour
{

    public float speed = 2f;
    public float rotationSpeed = 5f;

    private Animator animator;
    private bool canMove = true;

    // direction received from Input System (VR sticks)
    private Vector2 moveInput = Vector2.zero;

    void Start()
    {

        animator = GetComponent<Animator>();

        // Root motion must stay disabled
        animator.applyRootMotion = false;
    }

    void Update()
    {

        if (!canMove) return;

        Vector3 direction =
            new Vector3(
                moveInput.x,
                0,
                moveInput.y
            );

        bool isMoving =
            direction.magnitude > 0.1f;

        // Animation parameter in English
        animator.SetBool(
            "isWalking",
            isMoving
        );

        if (isMoving)
        {

            direction.Normalize();

            // Real movement controlled by script
            transform.position +=
                direction * speed *
                Time.deltaTime;

            Quaternion target =
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    target,
                    rotationSpeed *
                    Time.deltaTime
                );
        }
    }

    // 🎯 INPUT SYSTEM CALLBACK — this replaces GetAxis
    public void OnMove(InputValue value)
    {

        moveInput =
            value.Get<Vector2>();
    }

    public void SetMovement(bool enable)
    {
        canMove = enable;
    }
}
