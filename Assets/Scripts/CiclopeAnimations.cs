using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CiclopeAnimations : MonoBehaviour
{

    private Animator animator;
    private CiclopeMovement movement;
    private bool isDead = false;

    void Start()
    {

        animator =
            GetComponent<Animator>();

        movement =
            GetComponent<
                CiclopeMovement
            >();

        animator.applyRootMotion =
            false;
    }

    void Update()
    {

        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            animator.SetTrigger("Attack1");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            animator.SetTrigger("Attack2");

        if (Input.GetKeyDown(KeyCode.Space))
            animator.SetTrigger("JumpAttack");

        if (Input.GetKeyDown(KeyCode.K))
            Die();
    }

    public void PlayAttack1()
    {
        animator.SetTrigger("Attack1");
    }

    public void PlayAttack2()
    {
        animator.SetTrigger("Attack2");
    }

    public void PlayJumpAttack()
    {
        animator.SetTrigger("JumpAttack");
    }

    public void Die()
    {

        if (isDead) return;

        isDead = true;

        animator.SetTrigger(
            "Die"
        );

        if (movement != null)
            movement.enabled = false;

        Destroy(gameObject, 5f);
    }
}
