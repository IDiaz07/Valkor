using UnityEngine;
using System.Collections.Generic;

public static class CleanWireframeBaker
{
    public static Mesh Bake(Mesh sourceMesh, float angleThreshold = 0.99f)
    {
        if (sourceMesh == null) return null;

        Mesh mesh = Object.Instantiate(sourceMesh);
        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;
        BoneWeight[] weights = mesh.boneWeights;

        int vertexCount = tris.Length;
        Vector3[] newVerts = new Vector3[vertexCount];
        Vector3[] newNormals = new Vector3[vertexCount];
        BoneWeight[] newWeights = new BoneWeight[vertexCount];
        Color[] newColors = new Color[vertexCount];

        // WE ADD THIS: A dedicated channel for edge visibility
        Vector2[] newUV2 = new Vector2[vertexCount];
        Vector2[] newUV3 = new Vector2[vertexCount];

        int[] newTris = new int[vertexCount];

        Vector3[] triNormals = new Vector3[tris.Length / 3];
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = verts[tris[i]];
            Vector3 v1 = verts[tris[i + 1]];
            Vector3 v2 = verts[tris[i + 2]];
            triNormals[i / 3] = Vector3.Cross(v1 - v0, v2 - v0).normalized;
        }

        Dictionary<string, List<int>> edgeToTriangles = new Dictionary<string, List<int>>();
        for (int i = 0; i < tris.Length; i += 3)
        {
            AddEdge(tris[i], tris[i + 1], i / 3, edgeToTriangles);
            AddEdge(tris[i + 1], tris[i + 2], i / 3, edgeToTriangles);
            AddEdge(tris[i + 2], tris[i], i / 3, edgeToTriangles);
        }

        for (int i = 0; i < tris.Length; i += 3)
        {
            // Standard Barycentric Colors (1,0,0), (0,1,0), (0,0,1)
            newColors[i] = new Color(1, 0, 0, 1);
            newColors[i + 1] = new Color(0, 1, 0, 1);
            newColors[i + 2] = new Color(0, 0, 1, 1);

            // Determine if each edge of THIS triangle is visible
            // Edge X is opposite to Vertex 0 (so between v1 and v2)
            float visX = IsEdgeVisible(tris[i + 1], tris[i + 2], i / 3, edgeToTriangles, triNormals, angleThreshold) ? 1f : 0f;
            // Edge Y is opposite to Vertex 1 (so between v2 and v0)
            float visY = IsEdgeVisible(tris[i + 2], tris[i], i / 3, edgeToTriangles, triNormals, angleThreshold) ? 1f : 0f;
            // Edge Z is opposite to Vertex 2 (so between v0 and v1)
            float visZ = IsEdgeVisible(tris[i], tris[i + 1], i / 3, edgeToTriangles, triNormals, angleThreshold) ? 1f : 0f;

            // Apply the EXACT SAME visibility mask to all 3 vertices of the triangle!
            for (int j = 0; j < 3; j++)
            {
                int index = i + j;
                int oldIdx = tris[index];

                newVerts[index] = verts[oldIdx];
                newNormals[index] = normals[oldIdx];
                if (weights.Length > 0) newWeights[index] = weights[oldIdx];

                // Pack visX and visY into UV2, and visZ into UV3
                newUV2[index] = new Vector2(visX, visY);
                newUV3[index] = new Vector2(visZ, 0);

                newTris[index] = index;
            }
        }

        mesh.vertices = newVerts;
        mesh.normals = newNormals;
        mesh.boneWeights = newWeights;
        mesh.colors = newColors;
        mesh.uv2 = newUV2; // Store visibility here
        mesh.uv3 = newUV3;
        mesh.triangles = newTris;
        mesh.bindposes = sourceMesh.bindposes;

        return mesh;
    }

    private static void AddEdge(int v1, int v2, int triIdx, Dictionary<string, List<int>> dict)
    {
        string key = v1 < v2 ? v1 + "_" + v2 : v2 + "_" + v1;
        if (!dict.ContainsKey(key)) dict[key] = new List<int>();
        dict[key].Add(triIdx);
    }

    private static bool IsEdgeVisible(int v1, int v2, int triIdx, Dictionary<string, List<int>> dict, Vector3[] triNormals, float threshold)
    {
        string key = v1 < v2 ? v1 + "_" + v2 : v2 + "_" + v1;
        if (dict.TryGetValue(key, out List<int> tris))
        {
            if (tris.Count > 1)
            {
                int neighborIdx = (tris[0] == triIdx) ? tris[1] : tris[0];
                float dot = Vector3.Dot(triNormals[triIdx], triNormals[neighborIdx]);

                // If perfectly flat, hide the edge (return false)
                if (dot >= threshold) return false;
            }
        }
        return true; // Edge is visible
    }
}