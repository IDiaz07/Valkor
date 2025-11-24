using UnityEngine;

public class EnemyVisionSensor : MonoBehaviour
{
    [Header("Configuración de Visión")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float visionAngle = 45f; // Ángulo del cono (en grados)
    [SerializeField] private LayerMask obstacleMask; // Para objetos que bloquean visión

    [Header("Referencias")]
    [SerializeField] private Transform eyePosition; // Punto desde donde "ve" el enemigo

    private Transform detectedPlayer;

    public Transform DetectedPlayer => detectedPlayer;
    public bool HasPlayerInSight => detectedPlayer != null;

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        // Buscar todos los objetos con tag "Player" en el rango
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        detectedPlayer = null;

        foreach (GameObject playerObj in players)
        {
            Vector3 directionToPlayer = (playerObj.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, playerObj.transform.position);

            // Verificar si está dentro del rango
            if (distanceToPlayer > visionRange) continue;

            // Verificar si está dentro del ángulo de visión
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > visionAngle) continue;

            // Verificar línea de visión (sin obstáculos)
            Vector3 rayOrigin = eyePosition != null ? eyePosition.position : transform.position + Vector3.up;

            if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, visionRange, ~obstacleMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    detectedPlayer = playerObj.transform;
                    return;
                }
            }
        }
    }

    // Visualización del cono de visión en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = HasPlayerInSight ? Color.red : Color.yellow;

        Vector3 origin = eyePosition != null ? eyePosition.position : transform.position + Vector3.up;

        // Dibujar el rango de visión
        Gizmos.DrawWireSphere(origin, visionRange);

        // Dibujar el cono de visión
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle, 0) * transform.forward * visionRange;

        Gizmos.DrawLine(origin, origin + leftBoundary);
        Gizmos.DrawLine(origin, origin + rightBoundary);

        // Si detectó al jugador, dibujar línea hacia él
        if (HasPlayerInSight && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, DetectedPlayer.position);
        }
    }
}