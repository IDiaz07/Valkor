using UnityEngine;
using Unity.Netcode;

public class NetworkHeadCulling : NetworkBehaviour 
{
    [SerializeField] float m_DistanciaRecorte = 0.2f;

    // Esto hará que funcione en cuanto le des al Play, sin esperar a la red
    void Start()
    {
        AplicarRecorte();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            AplicarRecorte();
        }
    }

    void AplicarRecorte()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.nearClipPlane = m_DistanciaRecorte;
            Debug.Log("Recorte aplicado: " + m_DistanciaRecorte);
        }
    }
}