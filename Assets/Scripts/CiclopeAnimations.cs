using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CiclopeAnimations : MonoBehaviour
{

    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
    }

    public void PlayAttack1()
    {

        if (isDead) return;

        animator.SetTrigger("Attack1");
    }

    public void PlayAttack2()
    {

        if (isDead) return;

        animator.SetTrigger("Attack2");
    }

    public void PlayJumpAttack()
    {

        if (isDead) return;

        animator.SetTrigger("JumpAttack");
    }

    public void Die()
    {

        if (isDead) return;

        isDead = true;

        animator.SetTrigger("Die");
    }
}
