using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.Netcode;

public class NetworkAvatarSetup : NetworkBehaviour 
{
    [Header("Referencias")]
    [SerializeField] Transform m_HuesoCabeza; // Arrastra mixamorig:Head
    [SerializeField] IKTargetFollowVRRig m_IkScript; // El script de Valem

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // --- ESTE SOY YO (JUGADOR LOCAL) ---
            
            // 1. Encendemos el IK para que el cuerpo siga mis manos/cabeza
            if (m_IkScript != null) m_IkScript.enabled = true;

            // 2. Colapsamos la cabeza para no ver el interior del modelo
            if (m_HuesoCabeza != null) m_HuesoCabeza.localScale = Vector3.zero;

            // 3. Ajustamos la cámara para ver bien los brazos
            XROrigin rig = FindFirstObjectByType<XROrigin>();
            if (rig != null && rig.Camera != null)
            {
                rig.Camera.nearClipPlane = 0.01f;
            }
        }
        else
        {
            // --- ESTE ES OTRO JUGADOR (REMOTO) ---

            // 1. APAGAMOS su IK. 
            // Queremos que su cuerpo se mueva por el NetworkTransform,
            // no que intente "escuchar" a mis mandos.
            if (m_IkScript != null) m_IkScript.enabled = false;

            // 2. Nos aseguramos de ver su cabeza perfectamente
            if (m_HuesoCabeza != null) m_HuesoCabeza.localScale = Vector3.one;
        }
    }
}