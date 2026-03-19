using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.Netcode;

public class SimplePlayerAvatar : NetworkBehaviour 
{
    [SerializeField] Transform m_HeadTransform; 
    [SerializeField] GameObject m_VisualsDeLaCabeza; // Arrastra aquí el objeto que tiene el Mesh/Modelo
    
    private Transform m_HeadOrigin;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) 
        {
            BuscarCamara();
            // TRUCO MAESTRO: Si soy el dueño, apago mis visuales para no ver mi cara por dentro
            // Pero como soy el DUEÑO local, esto solo ocurre en MI pantalla.
            // Los demás verán mi cabeza porque en SUS pantallas IsOwner será falso para mi avatar.
            if(m_VisualsDeLaCabeza != null) 
                m_VisualsDeLaCabeza.SetActive(false); 
        }
    }

    void LateUpdate()
    {
        if (!IsOwner) return; 

        if (m_HeadOrigin == null) 
        {
            BuscarCamara();
            return;
        }

        if (m_HeadTransform != null)
        {
            m_HeadTransform.position = m_HeadOrigin.position;
            m_HeadTransform.rotation = m_HeadOrigin.rotation;
        }
    }

    void BuscarCamara()
    {
        XROrigin rig = FindFirstObjectByType<XROrigin>();
        if (rig != null && rig.Camera != null)
        {
            m_HeadOrigin = rig.Camera.transform;
        }
    }
}