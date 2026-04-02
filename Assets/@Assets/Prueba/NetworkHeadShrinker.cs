using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerSetup : NetworkBehaviour 
{
    [SerializeField] GameObject m_ModeloVisual; // El objeto CH32
    [SerializeField] Transform m_HuesoCabeza;

    public override void OnNetworkSpawn()
    {
        IKTargetFollowVRRig ikScript = GetComponent<IKTargetFollowVRRig>();

        if (IsOwner)
        {
            // SOY YO: Activo el IK y escondo mi cabeza
            if (ikScript != null) ikScript.enabled = true;
            if (m_HuesoCabeza != null) m_HuesoCabeza.localScale = Vector3.zero;
            
            // Ajusto mi cámara local (opcional si ya lo tienes en otro script)
            Camera.main.nearClipPlane = 0.01f;
        }
        else
        {
            // ES OTRO: Apago su IK para que no intente seguir mis manos
            // Su posición se sincronizará por el NetworkTransform
            if (ikScript != null) ikScript.enabled = false;
            
            // Me aseguro de ver su cabeza
            if (m_HuesoCabeza != null) m_HuesoCabeza.localScale = Vector3.one;
        }
    }
}