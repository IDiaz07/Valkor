using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.Netcode;

public class NetworkAvatarSetup : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform m_HuesoCabeza;
    [SerializeField] IKTargetFollowVRRig m_IkScript;

    // Mantener IK siempre desactivado hasta OnNetworkSpawn
    private void Awake()
    {
        // Desactivamos IK en Awake para evitar que corra sin dueño asignado
        if (m_IkScript != null) m_IkScript.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // SOY EL JUGADOR LOCAL
            if (m_IkScript != null) m_IkScript.enabled = true;
            if (m_HuesoCabeza != null) m_HuesoCabeza.localScale = Vector3.zero;

            // Busca el XROrigin real de la escena (no el del prefab)
            XROrigin rig = FindFirstObjectByType<XROrigin>();
            if (rig != null && rig.Camera != null)
            {
                rig.Camera.nearClipPlane = 0.01f;
            }
        }
        else
        {
            // ES UN JUGADOR REMOTO
            if (m_IkScript != null) m_IkScript.enabled = false;
            if (m_HuesoCabeza != null) m_HuesoCabeza.localScale = Vector3.one;
        }
    }
}