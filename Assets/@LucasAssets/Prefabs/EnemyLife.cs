using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float damagePerHit = 7f;
    [SerializeField] private string deathAnimationName = "Death"; // Nombre del estado en el Animator

    private float currentHealth;
    private bool isDead = false;
    private HashSet<Collider> weaponsInContact = new HashSet<Collider>();
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            if (!weaponsInContact.Contains(other))
            {
                TakeDamage(damagePerHit);
                weaponsInContact.Add(other);
                Debug.Log($"Golpeado por {other.gameObject.name}. Vida: {currentHealth}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            weaponsInContact.Remove(other);
        }
    }

    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} ha muerto");

        DisableEnemyComponents();

        // Reproduce la animación
        if (animator != null)
        {
            animator.SetTrigger("die");
        }

        DisableEnemyComponents();

        // Espera a que termine la animación
        StartCoroutine(WaitForDeathAnimation());
    }



    private void DisableEnemyComponents()
    {
        // Desactiva el PatrolController primero (esto es crítico)
        PatrolController patrol = GetComponent<PatrolController>();
        if (patrol != null)
        {
            patrol.enabled = false;
            Debug.Log("PatrolController desactivado");
        }

        // NavMeshAgent - CRÍTICO: verificar isOnNavMesh PRIMERO
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            // Solo manipula el agente si está en el NavMesh
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                Debug.Log("✓ NavMeshAgent detenido");
            }

            // Siempre desactívalo al final
            agent.enabled = false;
            Debug.Log("✓ NavMeshAgent desactivado");
        }

        // Desactiva el collider para no recibir más golpes
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("Collider desactivado");
        }

        // Si tienes EnemyVisionSensor, desactívalo también
        EnemyVisionSensor visionSensor = GetComponent<EnemyVisionSensor>();
        if (visionSensor != null)
        {
            visionSensor.enabled = false;
            Debug.Log("VisionSensor desactivado");
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Espera un frame para asegurarse de que la animación comenzó
        yield return null;

        // Espera hasta que el Animator esté en el estado de muerte
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationName))
        {
            yield return null;
        }

        // Ahora espera hasta que la animación termine
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        Debug.Log("Animación de muerte completada");

        // AHORA sí destruye el objeto
        Destroy(this.gameObject);
    }
}
