using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class CiclopeController : MonoBehaviour
{

    public Transform player;

    [Header("Wake Up Settings")]
    public float wakeDistance = 5f;
    public float waitAfterWakeUp = 3f;

    [Header("Chase & Attack")]
    public float chaseSpeed = 2f;
    public float attackDistance = 2f;
    public float attackCooldown = 2.5f;

    private Animator animator;
    private NavMeshAgent agent;
    private CiclopeWeaponHandler weaponHandler;
    private CiclopeAnimations animations;

    private bool hasWoken = false;
    private bool canChase = false;
    private bool isDead = false;

    private float wakeTimer = 0f;
    private float attackTimer = 0f;

    void Start()
    {

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        animations = GetComponent<CiclopeAnimations>();
        weaponHandler = GetComponent<CiclopeWeaponHandler>();

        if (player == null)
        {

            GameObject p =
                GameObject.FindWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        agent.speed = chaseSpeed;
    }

    void Update()
    {

        if (isDead ||
            player == null)
            return;

        float dist =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (!hasWoken &&
            dist <=
            wakeDistance)
        {

            hasWoken = true;

            wakeTimer =
                0f;

            canChase =
                false;

            animator.SetTrigger(
                "WakeUp"
            );

            animations =
                GetComponent<
                    CiclopeAnimations
                >();

            return;
        }

        if (hasWoken &&
            !canChase)
        {

            wakeTimer +=
                Time.deltaTime;

            if (wakeTimer >=
                waitAfterWakeUp)
            {

                canChase =
                    true;

                animator.SetBool(
                    "isWalking",
                    false
                );
            }

            return;
        }

        if (canChase)
        {

            ChaseLogic(
                dist
            );

            AttackLogic(
                dist
            );
        }
    }

    void ChaseLogic(float dist)
    {

        if (agent ==
            null ||
            player ==
            null)
            return;

        agent.speed =
            chaseSpeed;

        agent.SetDestination(
            player.position
        );

        animator.SetBool(
            "isWalking",
            agent.velocity.magnitude > 0.2f
        );
    }

    void AttackLogic(float dist)
    {

        if (animations ==
            null)
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

            if (r ==
                1)
                animations.PlayAttack1();

            if (r ==
                2)
                animations.PlayAttack2();

            if (r ==
                3)
                animations.PlayJumpAttack();

            attackTimer =
                0f;
        }
    }

    public void ForceDie()
    {

        isDead =
            true;

        animator.SetTrigger(
            "Die"
        );

        if (agent != null)
            agent.enabled =
            false;

        Destroy(
            gameObject,
            5f
        );
    }

    void OnTriggerEnter(Collider other)
    {

        if (
            other.gameObject.layer ==
            LayerMask.NameToLayer(
                "Weapon"
            )
        )
        {

            CiclopeHealth h =
                GetComponent<
                    CiclopeHealth
                >();

            if (h != null)
                h.TakeDamage(
                    30f
                );
        }
    }

    public float GetDistanceToPlayer()
    {

        if (player ==
            null)
            return 999f;

        return Vector3.Distance(
            transform.position,
            player.position
        );
    }
}
