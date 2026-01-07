using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class CiclopeController : MonoBehaviour
{

    public Transform player;

    public float wakeDistance = 5f;
    public float chaseSpeed = 2f;
    public float attackDistance = 2f;

    private Animator animator;
    private NavMeshAgent agent;
    private CiclopeAnimations animations;

    private bool hasWoken = false;
    private bool isDead = false;

    private float attackTimer = 0f;
    public float attackCooldown = 2.5f;

    void Start()
    {

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animations = GetComponent<CiclopeAnimations>();

        animator.applyRootMotion = false;

        if (player == null)
        {

            GameObject p =
                GameObject.FindWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        agent.speed = chaseSpeed;
        agent.stoppingDistance = 2f;
    }

    void Update()
    {

        if (isDead || player == null)
            return;

        float dist =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // WAKE UP WHEN VR PLAYER IS CLOSE
        if (!hasWoken &&
            dist <= wakeDistance)
        {

            hasWoken = true;

            animator.SetTrigger("WakeUp");

            return;
        }

        // AFTER WAKE UP → CHASE / ATTACK
        if (hasWoken)
        {

            if (dist >
                attackDistance)
            {

                HandleChase();

            }
            else
            {

                HandleAttackByDistance(dist);
            }
        }
    }

    // 🎯 SOLO 1 DEFINICIÓN — PERSEGUIR
    void HandleChase()
    {

        agent.SetDestination(
            player.position
        );

        animator.SetBool(
            "isWalking",
            agent.velocity.magnitude > 0.2f
        );
    }

    // 🎯 SOLO 1 DEFINICIÓN — ATAQUE
    void HandleAttackByDistance(float dist)
    {

        if (animations == null)
            return;

        attackTimer +=
            Time.deltaTime;

        if (attackTimer <
            attackCooldown)
            return;

        if (dist <=
            attackDistance)
        {

            int r =
                Random.Range(
                    1,
                    4
                );

            if (r == 1)
                animations.PlayAttack1();

            if (r == 2)
                animations.PlayAttack2();

            if (r == 3)
                animations.PlayJumpAttack();

            attackTimer = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer ==
            LayerMask.NameToLayer("Weapon"))
        {

            CiclopeHealth h =
                GetComponent<CiclopeHealth>();

            if (h != null)
                h.TakeDamage(30f);
        }
    }

    public void Die()
    {

        isDead = true;

        animator.SetTrigger("Die");

        if (agent != null)
            agent.enabled = false;

        if (animations != null)
            animations.enabled = false;

        Destroy(
            gameObject,
            5f
        );
    }
}
