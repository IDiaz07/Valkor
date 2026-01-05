using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class CaveOreSpawner : MonoBehaviour
{
    [Header("Ore Settings")]
    public List<GameObject> orePrefabs;
    public int oreCount = 100;

    [Header("Spawn Settings")]
    public float surfaceOffset = 0.02f;
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("Raycast")]
    public float rayDistance = 50f;
    public LayerMask caveLayer;

    [Header("Noise")]
    public float noiseScale = 0.2f;
    [Range(0f, 1f)] public float noiseThreshold = 0.2f;

    [Header("Debug")]
    public bool drawDebugRays = false;

    private MeshCollider meshCollider;

    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();

        if (meshCollider == null)
        {
            Debug.LogError("❌ No MeshCollider en la cueva");
            return;
        }

        if (orePrefabs == null || orePrefabs.Count == 0)
        {
            Debug.LogError("❌ No hay prefabs de mena asignados");
            return;
        }

        SpawnOres();
    }

    void SpawnOres()
    {
        Bounds b = meshCollider.bounds;

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = oreCount * 20;

        Debug.Log("⛏️ CaveOreSpawner_AnyDirection iniciado");

        while (spawned < oreCount && attempts < maxAttempts)
        {
            attempts++;

            // 1️⃣ Punto aleatorio dentro del volumen de la cueva
            Vector3 randomPoint = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y),
                Random.Range(b.min.z, b.max.z)
            );

            // 2️⃣ Dirección aleatoria
            Vector3 dir = Random.onUnitSphere;

            if (drawDebugRays)
                Debug.DrawRay(randomPoint, dir * rayDistance, Color.cyan, 5f);

            // 3️⃣ Raycast
            if (!Physics.Raycast(randomPoint, dir, out RaycastHit hit, rayDistance, caveLayer))
                continue;

            // 4️⃣ Ruido simple (opcional)
            float noise = Mathf.PerlinNoise(
                hit.point.x * noiseScale,
                hit.point.z * noiseScale
            );
            if (noise < noiseThreshold)
                continue;

            // 5️⃣ Instanciar mena
            GameObject prefab = orePrefabs[Random.Range(0, orePrefabs.Count)];

            GameObject ore = Instantiate(
                prefab,
                hit.point + hit.normal * surfaceOffset,
                Quaternion.FromToRotation(Vector3.up, hit.normal),
                transform
            );

            ore.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
            ore.transform.localScale =
                Vector3.one * Random.Range(scaleRange.x, scaleRange.y);

            spawned++;
        }

        Debug.Log($"✅ Menas colocadas: {spawned} (intentos {attempts})");
    }
}
