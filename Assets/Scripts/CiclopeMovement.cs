using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CiclopeMovement : MonoBehaviour
{

    public float speed = 2f;
    public float rotationSpeed = 5f;

    private Animator animator;
    private bool canMove = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
    }

    void Update()
    {

        if (!canMove) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction =
            new Vector3(h, 0, v);

        bool isMoving =
            direction.magnitude > 0.1f;

        animator.SetBool(
            "isWalking",
            isMoving
        );

        if (isMoving)
        {

            direction.Normalize();

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

    public void SetMovement(bool enable)
    {
        canMove = enable;
    }
}
