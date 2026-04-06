#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class WireframeEditorBaker : MonoBehaviour
{
    [ContextMenu("Bake and Save Wireframe Mesh")]
    public void BakeAndSave()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("No MeshFilter or Mesh found!");
            return;
        }

        // 1. Use your existing baker script logic here to generate the mesh
        Mesh wireMesh = CleanWireframeBaker.Bake(mf.sharedMesh, 0.999f);

        // 2. Give it a name
        wireMesh.name = mf.sharedMesh.name + "_Wireframe";

        // 3. Save it permanently to your hard drive!
        string path = $"Assets/{wireMesh.name}.asset";
        AssetDatabase.CreateAsset(wireMesh, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();

        // 4. Automatically assign the new saved mesh to this object
        mf.sharedMesh = wireMesh;

        Debug.Log($"Successfully baked and saved wireframe mesh to: {path}");
    }
}
#endif