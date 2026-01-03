using System.Collections;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrolling,
    Chasing,
    Returning,
    Fighting
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

    [Header("Configuración de Combate")]
    [SerializeField] private float fightingDistance = 0f;//1.5f;        // Distancia realista para entrar en combate
    [SerializeField] private float fightingStopDistance = 0f;      // Distancia para salir de combate
    [SerializeField] private float fightingRotationSpeed = 5f;
    [SerializeField] private float minimumChaseTime = 0.5f;
    [SerializeField] private float minTimeBetweenHits = 1f;        // Tiempo mínimo entre golpes
    [SerializeField] private float maxTimeBetweenHits = 3f;

    [SerializeField] private Collider golpe;

    [Header("Sistema de Daño")]
    [SerializeField] private float hitStunDuration = 0.5f;    // Tiempo que dura el stun al recibir golpe
    private bool isStunned = false;                           // Si está aturdido por un golpe
    private float stunnedTimer = 0f;

    [Header("Referencias")]
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Estado Interno")]
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private EnemyState currentState = EnemyState.Patrolling;
    private Vector3 lastKnownPlayerPosition;
    private bool wasInFightingState = false;
    private float timeInCurrentState = 0f;  // Contador de tiempo en el estado actual
    private float nextHitTime = 0f;     //Momento del próximo golpe
    private float timeSinceEnteredFighting = 0f;


    void Start()
    {
        // Obtener componentes
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        animator = this.gameObject.GetComponent<Animator>();

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

        // DESPUÉS (busca solo en los hijos de este enemigo):
        Transform golpeTransform = transform.Find("Golpe");

        // Si no está en hijo directo, buscar recursivamente
        if (golpeTransform == null)
        {
            golpeTransform = GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t.name == "Golpe");
        }

        if (golpeTransform != null)
        {
            golpe = golpeTransform.GetComponent<Collider>();
            Debug.Log($"[{gameObject.name}] Golpe encontrado: {golpe.GetInstanceID()}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No se encontró 'Golpe' en los hijos");
        }

        Debug.Log($"El golpe lo da: {golpe}");
    }


    void Update()
    {
        if (patrolPoints.Length == 0) return;

        timeInCurrentState += Time.deltaTime;

        // NUEVO: Manejar estado de stun
        if (isStunned)
        {
            stunnedTimer -= Time.deltaTime;

            if (stunnedTimer <= 0f)
            {
                isStunned = false;
                Debug.Log("<color=green>Enemigo recuperado del stun</color>");

                // Reactivar el agente según el estado actual
                if (currentState == EnemyState.Chasing || currentState == EnemyState.Patrolling || currentState == EnemyState.Returning)
                {
                    agent.isStopped = false;
                }
            }

            // Mientras está stunned, no procesar comportamiento normal
            return;
        }

        // TEST MANUAL
        /*if (Input.GetKeyDown(KeyCode.H) && currentState == EnemyState.Fighting) EL NUEVO SISTEMA DE INPUT DE UNITY DA ERROR AL COGER ASÍ EL INPUT DEL TECLADO
        {
            Debug.Log("<color=red>TEST MANUAL: Forzando golpe con tecla H</color>");
            PerformHit();
        }*/

        UpdateState();

        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Fighting:
                HandleFighting();
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
            float distanceToPlayer = Vector3.Distance(transform.position, visionSensor.DetectedPlayer.position);

            // 1. Si está patrullando o volviendo, SIEMPRE pasar primero a Chasing
            if (currentState == EnemyState.Patrolling || currentState == EnemyState.Returning)
            {
                EnterChaseState();
            }
            // 2. MODIFICADO: Solo puede entrar en Fighting si ha perseguido el tiempo suficiente Y está cerca
            else if (currentState == EnemyState.Chasing &&
                     timeInCurrentState >= minimumChaseTime &&      // <-- NUEVA CONDICIÓN
                     distanceToPlayer <= fightingDistance)
            {
                EnterFightingState();
            }
            // 3. Si está en Fighting y el jugador se aleja, volver a Chasing
            else if (currentState == EnemyState.Fighting && distanceToPlayer > fightingStopDistance)
            {
                EnterChaseState();
            }

            lastKnownPlayerPosition = visionSensor.DetectedPlayer.position;
        }
        else
        {
            // Perdió de vista al jugador
            if (currentState == EnemyState.Chasing || currentState == EnemyState.Fighting)
            {
                EnterReturningState();
            }
        }
    }

    void EnterFightingState()
    {
        currentState = EnemyState.Fighting;
        timeInCurrentState = 0f;  // <-- RESETEAR TIMER

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        timeSinceEnteredFighting = 0f;
        nextHitTime = Random.Range(minTimeBetweenHits, maxTimeBetweenHits);
        Debug.Log($"Entrando en Fighting. Primer golpe en: {nextHitTime:F2}s");

        Debug.Log("¡Enemigo entró en combate cuerpo a cuerpo!");
    }

    void HandleFighting()
    {
        // Verificar que el sensor sigue detectando al jugador
        if (visionSensor == null || !visionSensor.HasPlayerInSight)
        {
            return;
        }

        // Incrementar el tiempo en combate
        timeSinceEnteredFighting += Time.deltaTime;

        // Mantener al enemigo rotando hacia el jugador
        Vector3 directionToPlayer = (visionSensor.DetectedPlayer.position - transform.position).normalized;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * fightingRotationSpeed);
        }

        // Sistema de golpes aleatorios
        if (timeSinceEnteredFighting >= nextHitTime)
        {
            PerformHit();

            // RESETEAR el timer y calcular el próximo golpe
            timeSinceEnteredFighting = 0f;  // <-- CLAVE: Resetear a 0
            nextHitTime = Random.Range(minTimeBetweenHits, maxTimeBetweenHits);

            Debug.Log($"Golpe ejecutado. Próximo golpe en: {nextHitTime:F2}s");
        }
    }

    void EnterChaseState()
    {
        currentState = EnemyState.Chasing;
        timeInCurrentState = 0f;

        agent.speed = chaseSpeed;
        agent.autoBraking = true;
        agent.stoppingDistance = 0.5f; 
        agent.isStopped = false;
        isWaiting = false;

        Debug.Log("¡Enemigo detectó al jugador!");
    }

    void PerformHit()
    {
        animator.SetTrigger("isHitting");
        Debug.Log($"[{Time.time:F2}] ¡Enemigo golpea!");

        //TODO
        StartCoroutine(ManageAttackCollider());
    }

    private IEnumerator ManageAttackCollider()
    {
        // Activar collider al inicio
        if (golpe != null)
        {
            golpe.enabled = true;
            Debug.Log($"Collider activado en {Time.time}");
        }

        // Esperar 3.4 segundos
        yield return new WaitForSeconds(3f);

        // Desactivar collider
        if (golpe != null)
        {
            golpe.enabled = false;
            Debug.Log($"Collider desactivado en {Time.time}");
        }
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
        if (currentState == EnemyState.Fighting)
        {
            animator.SetTrigger("isNotWalking");
        }

        currentState = EnemyState.Returning;
        timeInCurrentState = 0f;  // RESETEAR TIMER

        agent.speed = patrolSpeed;
        agent.autoBraking = false;
        agent.isStopped = false;

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
        if (currentState == EnemyState.Fighting)
        {
            // ACTIVAR el trigger continuamente mientras esté en Fighting
            // Unity lo reactivará cada vez que la animación termine
            animator.SetTrigger("isFighting");

            // Marcar que estamos en Fighting (solo para tracking)
            if (!wasInFightingState)
            {
                wasInFightingState = true;
                Debug.Log("Entrando en animación de combate");
            }
        }
        else
        {
            // Salimos del estado Fighting
            if (wasInFightingState)
            {
                animator.SetTrigger("isNotWalking");
                wasInFightingState = false;
                Debug.Log("Saliendo de animación de combate");
            }

            // Lógica normal de caminar (solo cuando NO está en Fighting)
            bool hasValidPath = agent.hasPath && !agent.pathPending;
            bool isMovingToDestination = agent.remainingDistance > agent.stoppingDistance;
            bool hasVelocity = agent.velocity.sqrMagnitude > 0.0001f;
            bool shouldWalk = hasValidPath && isMovingToDestination && hasVelocity && !isWaiting;

            if (shouldWalk)
            {
                animator.SetTrigger("isWalking");
            }
            else
            {
                animator.SetTrigger("isNotWalking");
            }
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
        if (distanceToTarget <= agent.stoppingDistance)
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

    void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que colisionó está en la capa Weapon
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            Debug.Log($"<color=red>¡Enemigo golpeado por: {other.gameObject.name}!</color>");
            OnHitByWeapon();
        }
    }

    void OnHitByWeapon()
    {
        // Activar animación de golpeado
        animator.SetTrigger("beenHitted");

        // Activar estado de stun
        isStunned = true;
        stunnedTimer = hitStunDuration;

        // Detener movimiento temporalmente
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Debug.Log($"<color=orange>Enemigo recibió golpe. Stunned por {hitStunDuration}s</color>");
    }
}