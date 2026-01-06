using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Gestor simple de enemigos que mantiene un número constante en el mapa
/// </summary>
public class EnemySpawn : MonoBehaviour
{
    [Header("Configuración Básica")]
    [SerializeField] private GameObject[] enemyPrefabs; // Lista de prefabs de enemigos
    [SerializeField] private int numberOfEnemies = 5;
    [SerializeField] private Transform[] spawnPoints; // 4 puntos de spawn

    [Header("Tiempo de Respawn")]
    [SerializeField] private float respawnDelay = 2f;

    [Header("Altura de Spawn")]
    [SerializeField] private float spawnHeight = 2f; // Altura extra para que caigan

    [Header("Patrullaje")]
    [SerializeField] private bool assignWaypoints = true; // Activar/desactivar asignación automática
    [SerializeField] private Transform[] patrolWaypoints; // Todos los checkpoints/waypoints

    // Lista de enemigos activos
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        // Validar que haya prefabs asignados
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No hay prefabs de enemigos asignados!");
            enabled = false;
            return;
        }

        // Validar que haya spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No hay puntos de spawn asignados!");
            enabled = false;
            return;
        }

        // Spawnear los enemigos iniciales
        for (int i = 0; i < numberOfEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        // Limpiar enemigos muertos de la lista
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Si faltan enemigos, respawnear
        if (activeEnemies.Count < numberOfEnemies)
        {
            Invoke(nameof(SpawnEnemy), respawnDelay);
        }
    }

    void SpawnEnemy()
    {
        // Elegir un punto de spawn aleatorio
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Calcular posición con altura extra
        Vector3 spawnPosition = spawnPoint.position + Vector3.up * spawnHeight;

        // Elegir un prefab de enemigo aleatorio
        GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Crear el enemigo en la posición elevada
        GameObject newEnemy = Instantiate(randomPrefab, spawnPosition, spawnPoint.rotation);

        // Asignar waypoints si está configurado
        if (assignWaypoints && patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            AssignWaypointsToEnemy(newEnemy);
        }

        // Añadir a la lista
        activeEnemies.Add(newEnemy);

        Debug.Log($"Enemigo {randomPrefab.name} spawneado en {spawnPoint.name} (+{spawnHeight}m altura). Total: {activeEnemies.Count}/{numberOfEnemies}");
    }

    /// <summary>
    /// Asigna los waypoints de patrullaje al enemigo
    /// </summary>
    void AssignWaypointsToEnemy(GameObject enemy)
    {
        PatrolController patrol = enemy.GetComponent<PatrolController>();

        if (patrol == null)
        {
            Debug.LogWarning($"El enemigo {enemy.name} no tiene componente PatrolController");
            return;
        }

        // Intentar usar el método público SetPatrolPoints (si existe)
        System.Reflection.MethodInfo method = typeof(PatrolController).GetMethod("SetPatrolPoints");

        if (method != null)
        {
            // Método público existe, usarlo (RECOMENDADO)
            method.Invoke(patrol, new object[] { patrolWaypoints });
            Debug.Log($"✓ Waypoints asignados a {enemy.name} usando SetPatrolPoints(): {patrolWaypoints.Length} puntos");
        }
        else
        {
            // Método público no existe, usar reflexión en el campo privado (FALLBACK)
            System.Reflection.FieldInfo field = typeof(PatrolController).GetField("patrolPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(patrol, patrolWaypoints);
                Debug.Log($"⚠ Waypoints asignados a {enemy.name} usando reflexión: {patrolWaypoints.Length} puntos");
                Debug.LogWarning("Recomendación: Añade el método SetPatrolPoints() a tu PatrolController para mejor rendimiento");
            }
            else
            {
                Debug.LogError($"✗ No se pudo asignar waypoints a {enemy.name}. Verifica que PatrolController tenga el campo 'patrolPoints'");
            }
        }
    }

    // Método opcional para cambiar el número de enemigos en tiempo real
    public void SetNumberOfEnemies(int newNumber)
    {
        numberOfEnemies = newNumber;
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Gizmos.color = Color.green;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                // Dibujar punto de spawn base
                Gizmos.DrawWireSphere(point.position, 0.5f);

                // Calcular posición con altura
                Vector3 elevatedPosition = point.position + Vector3.up * spawnHeight;

                // Dibujar punto de spawn elevado
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(elevatedPosition, 0.3f);

                // Línea conectando ambos puntos
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(point.position, elevatedPosition);

                // Flechas indicando caída
                Gizmos.color = Color.red;
                Gizmos.DrawLine(elevatedPosition, elevatedPosition + Vector3.down * 0.5f);
            }
        }
    }
}