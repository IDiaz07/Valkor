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

    void Start()
    {

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animations = GetComponent<CiclopeAnimations>();

        animator.applyRootMotion = false;

        // Auto find player
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

        // 1. Wake up only when player is close
        if (!hasWoken &&
            dist <= wakeDistance)
        {

            hasWoken = true;

            animator.SetTrigger("WakeUp");

            return;
        }

        // 2. After wake animation → chase / attack
        if (hasWoken)
        {

            if (dist >
                attackDistance)
            {

                ChaseAndAnimate();

            }
            else
            {

                AttackLogic();
            }
        }
    }

    // 🎯 ONLY ONE DEFINITION — WALK / CHASE
    void ChaseAndAnimate()
    {

        if (agent == null || player == null)
            return;

        agent.speed = chaseSpeed;

        agent.SetDestination(
            player.position
        );

        animator.SetBool(
            "isWalking",
            agent.velocity.magnitude > 0.2f
        );
    }

    // 🎯 ONLY ONE DEFINITION — ATTACK DECISION
    void AttackLogic()
    {

        if (animations == null)
            return;

        int r = Random.Range(1, 4);

        if (r == 1)
            animations.PlayAttack1();

        if (r == 2)
            animations.PlayAttack2();

        if (r == 3)
            animations.PlayJumpAttack();
    }

    // 🎯 COLLISION WITH WEAPON LAYER — IN ENGLISH
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

    // 🎯 DEATH
    public void Die()
    {

        if (isDead) return;

        isDead = true;

        animator.SetTrigger("Die");

        if (agent != null)
            agent.enabled = false;

        if (animations != null)
            animations.enabled = false;

        Destroy(gameObject, 5f);
    }

    public float GetDistanceToPlayer()
    {

        if (player == null)
            return 999f;

        return Vector3.Distance(
            transform.position,
            player.position
        );
    }
}
