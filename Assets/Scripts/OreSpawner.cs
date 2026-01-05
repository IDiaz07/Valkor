using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class OreSpawner : MonoBehaviour
{
    [Header("Ore Settings")]
    public List<GameObject> orePrefabs;
    public int oreCount = 2000;

    [Header("Height Range")]
    public float minHeight = 20f;
    public float maxHeight = 120f;

    [Header("Slope Settings")]
    [Range(0, 60)]
    public float maxSlope = 30f;

    [Header("Noise Settings")]
    public float noiseScale = 0.05f;
    [Range(0, 1)]
    public float noiseThreshold = 0.55f;

    [Header("Random Scale")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.3f);

    [Header("Placement")]
    public bool alignToNormal = true;
    public bool spawnOnStart = true;

    private Terrain terrain;
    private TerrainData terrainData;

    private void Start()
    {
        if (!spawnOnStart) return;

        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        SpawnOres();
    }

    public void SpawnOres()
    {
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = oreCount * 5;

        while (spawned < oreCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(0f, terrainSize.x);
            float z = Random.Range(0f, terrainSize.z);

            float normX = x / terrainSize.x;
            float normZ = z / terrainSize.z;

            float height = terrainData.GetInterpolatedHeight(normX, normZ);
            float steepness = terrainData.GetSteepness(normX, normZ);

            if (height < minHeight || height > maxHeight) continue;
            if (steepness > maxSlope) continue;

            float noise = Mathf.PerlinNoise(
                (x + terrainPos.x) * noiseScale,
                (z + terrainPos.z) * noiseScale
            );

            if (noise < noiseThreshold) continue;

            Vector3 worldPos = new Vector3(
                terrainPos.x + x,
                terrainPos.y + height,
                terrainPos.z + z
            );

            GameObject prefab = orePrefabs[Random.Range(0, orePrefabs.Count)];
            GameObject ore = Instantiate(prefab, worldPos, Quaternion.identity, transform);

            if (alignToNormal)
            {
                Vector3 normal = terrainData.GetInterpolatedNormal(normX, normZ);
                ore.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            }

            ore.transform.Rotate(Vector3.up, Random.Range(0f, 360f));

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            ore.transform.localScale = Vector3.one * scale;

            spawned++;
        }

        Debug.Log($"OreSpawner → Menas colocadas: {spawned}");
    }
}
