using UnityEngine;
using Unity.XR.CoreUtils;

public class PruebaCabezaLocal : MonoBehaviour
{
    [Range(0.01f, 0.3f)]
    public float distanciaRecorte = 0.18f;

    void Update()
    {
        // Esto lo hacemos en el Update para que puedas mover 
        // el slider en tiempo real mientras tienes las gafas puestas
        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = GetComponentInChildren<Camera>();

        if (cam != null)
        {
            cam.nearClipPlane = distanciaRecorte;
        }
    }
}