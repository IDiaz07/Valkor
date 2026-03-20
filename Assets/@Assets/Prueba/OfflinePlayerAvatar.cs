using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.Netcode;

public class SimplePlayerAvatar : NetworkBehaviour 
{
    [Header("Configuración de Red")]
    [SerializeField] Transform m_HeadTransform; 

    [Header("Modelos de Cabeza")]
    [SerializeField] GameObject m_ModeloHost;    // The Boss
    [SerializeField] GameObject m_ModeloCliente; // Claire

    private Transform m_HeadOrigin;
    private GameObject m_MiCabezaVisual;

    public override void OnNetworkSpawn()
    {
        // 1. DETERMINAR EL MODELO SEGÚN EL ID
        // El Host siempre tiene el OwnerClientId = 0
        if (OwnerClientId == 0) 
        {
            m_ModeloHost.SetActive(true);
            m_ModeloCliente.SetActive(false);
            m_MiCabezaVisual = m_ModeloHost;
        }
        else // Cualquier otro cliente (ID 1, 2, 3...) será Claire
        {
            m_ModeloHost.SetActive(false);
            m_ModeloCliente.SetActive(true);
            m_MiCabezaVisual = m_ModeloCliente;
        }

        // 2. VISIBILIDAD LOCAL (Si soy YO, no veo mi cabeza)
        if (IsOwner) 
        {
            BuscarCamara();
            if(m_MiCabezaVisual != null) 
                m_MiCabezaVisual.SetActive(false); 
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