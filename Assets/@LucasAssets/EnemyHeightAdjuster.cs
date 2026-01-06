using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Mantiene al enemigo ligeramente elevado del suelo para evitar problemas de colliders.
/// Útil para modelos que tienen problemas de penetración con el terrain.
/// </summary>
public class EnemyHeightAdjuster : MonoBehaviour
{
    [Header("Configuración de Altura")]
    [SerializeField] private float heightOffset = 0.1f; // Altura extra sobre el NavMesh
    [SerializeField] private bool adjustOnStart = true; // Ajustar al iniciar
    [SerializeField] private bool keepAdjusting = false; // Mantener ajustando cada frame

    private NavMeshAgent agent;
    private float originalBaseOffset;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // Guardar el offset original
            originalBaseOffset = agent.baseOffset;

            if (adjustOnStart)
            {
                AdjustHeight();
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No se encontró NavMeshAgent. EnemyHeightAdjuster desactivado.");
            enabled = false;
        }
    }

    void Update()
    {
        if (keepAdjusting && agent != null)
        {
            AdjustHeight();
        }
    }

    /// <summary>
    /// Ajusta la altura del enemigo
    /// </summary>
    void AdjustHeight()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.baseOffset = originalBaseOffset + heightOffset;
        }
    }

    /// <summary>
    /// Método público para cambiar la altura en runtime
    /// </summary>
    public void SetHeightOffset(float newOffset)
    {
        heightOffset = newOffset;
        AdjustHeight();
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // Línea mostrando la altura del offset
        Vector3 basePosition = transform.position;
        Vector3 offsetPosition = basePosition + Vector3.up * heightOffset;

        Gizmos.DrawLine(basePosition, offsetPosition);
        Gizmos.DrawWireSphere(offsetPosition, 0.1f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            offsetPosition + Vector3.up * 0.2f,
            $"Offset: +{heightOffset}m"
        );
#endif
    }
}