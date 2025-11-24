using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrolling,
    Chasing,
    Returning
}

public class PatrolController : MonoBehaviour
{
    [Header("Configuración de Patrullaje")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 2f;
    [SerializeField] private float patrolSpeed = 3f;

    [Header("Configuración de Persecución")]
    [SerializeField] private EnemyVisionSensor visionSensor;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float loseTargetDistance = 15f;

    [Header("Referencias")]
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Estado Interno")]
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private EnemyState currentState = EnemyState.Patrolling;
    private Vector3 lastKnownPlayerPosition;

    void Start()
    {
        // Obtener componentes
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Configuración inicial del NavMeshAgent
        agent.speed = patrolSpeed;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.5f;

        // Validación del sensor de visión
        if (visionSensor == null)
        {
            visionSensor = GetComponent<EnemyVisionSensor>();
            if (visionSensor == null)
            {
                Debug.LogError("No se encontró EnemyVisionSensor en " + gameObject.name);
            }
        }

        // Validación y comienzo
        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
        else
        {
            Debug.LogWarning("No hay puntos de patrullaje asignados en " + gameObject.name);
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0) return;

        // Actualizar estado según detección
        UpdateState();

        // Comportamiento según el estado
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Returning:
                HandleReturning();
                break;
        }

        UpdateAnimation();
    }

    void UpdateState()
    {
        if (visionSensor != null && visionSensor.HasPlayerInSight)
        {
            // Detectó al jugador
            if (currentState != EnemyState.Chasing)
            {
                EnterChaseState();
            }
            lastKnownPlayerPosition = visionSensor.DetectedPlayer.position;
        }
        else if (currentState == EnemyState.Chasing)
        {
            // Ya no ve al jugador - verificar si debe volver
            float distanceToLastKnown = Vector3.Distance(transform.position, lastKnownPlayerPosition);

            if (distanceToLastKnown > loseTargetDistance || agent.remainingDistance < 1f)
            {
                EnterReturningState();
            }
        }
    }

    void EnterChaseState()
    {
        currentState = EnemyState.Chasing;
        agent.speed = chaseSpeed;
        agent.autoBraking = true;
        isWaiting = false;

        Debug.Log("¡Enemigo detectó al jugador!");
    }

    void HandleChasing()
    {
        if (visionSensor != null && visionSensor.HasPlayerInSight)
        {
            agent.SetDestination(visionSensor.DetectedPlayer.position);
        }
    }

    void EnterReturningState()
    {
        currentState = EnemyState.Returning;
        agent.speed = patrolSpeed;
        agent.autoBraking = false;

        // Volver al punto de patrullaje más cercano
        int closestPointIndex = FindClosestPatrolPoint();
        currentPatrolIndex = closestPointIndex;
        GoToNextPatrolPoint();

        Debug.Log("Enemigo perdió al jugador, volviendo a patrullar");
    }

    void HandleReturning()
    {
        // Cuando llegue al punto, volver a patrullar
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            {
                currentState = EnemyState.Patrolling;
            }
        }
    }

    void HandlePatrolling()
    {
        if (isWaiting)
        {
            HandleWaiting();
            return;
        }

        CheckArrival();
    }

    int FindClosestPatrolPoint()
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;

            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    void UpdateAnimation()
    {
        bool hasValidPath = agent.hasPath && !agent.pathPending;
        bool isMovingToDestination = agent.remainingDistance > agent.stoppingDistance;
        bool hasVelocity = agent.velocity.sqrMagnitude > 0.0001f;

        bool shouldWalk = hasValidPath && isMovingToDestination && hasVelocity && !isWaiting;

        if (shouldWalk)
        {
            animator.SetTrigger("isWalking");
        }
    }

    void HandleWaiting()
    {
        waitTimer += Time.deltaTime;

        if (waitTimer >= waitTimeAtPoint)
        {
            isWaiting = false;
            waitTimer = 0f;
            GoToNextPatrolPoint();
        }
    }

    void CheckArrival()
    {
        // Salir si el path aún se está calculando
        if (agent.pathPending) return;

        // Calcular distancia directa al punto objetivo (más preciso)
        if (patrolPoints[currentPatrolIndex] == null) return;

        // Obtener el índice del punto actual (el que acabamos de establecer)
        int targetIndex = (currentPatrolIndex - 1 + patrolPoints.Length) % patrolPoints.Length;

        float distanceToTarget = Vector3.Distance(transform.position, patrolPoints[targetIndex].position);

        // Verificar si está suficientemente cerca del punto
        if (distanceToTarget <= agent.stoppingDistance + 0.5f)
        {
            // Verificar que está quieto o casi quieto
            if (agent.velocity.sqrMagnitude < 0.001f)
            {
                animator.SetTrigger("isNotWalking");
                Debug.Log($"Llegó al waypoint {targetIndex}");
                agent.ResetPath();
                StartWaiting();
            }
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        if (patrolPoints[currentPatrolIndex] == null)
        {
            Debug.LogError($"Punto de patrullaje {currentPatrolIndex} es null!");
            return;
        }

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void StartWaiting()
    {
        isWaiting = true;
        waitTimer = 0f;
    }

    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);

                Transform nextPoint = patrolPoints[(i + 1) % patrolPoints.Length];
                if (nextPoint != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, nextPoint.position);
                }

#if UNITY_EDITOR
                UnityEditor.Handles.Label(patrolPoints[i].position + Vector3.up * 0.5f, i.ToString());
#endif
            }
        }

        if (Application.isPlaying && patrolPoints.Length > 0)
        {
            int targetIndex = (currentPatrolIndex - 1 + patrolPoints.Length) % patrolPoints.Length;
            if (patrolPoints[targetIndex] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(patrolPoints[targetIndex].position, 0.4f);
            }
        }

        // Visualizar estado actual
        if (Application.isPlaying)
        {
            Vector3 textPosition = transform.position + Vector3.up * 2f;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(textPosition, $"Estado: {currentState}");
#endif
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && agent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, agent.destination);

            // Visualizar última posición conocida del jugador
            if (currentState == EnemyState.Chasing || currentState == EnemyState.Returning)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
            }
        }
    }
}