using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.Netcode;

public class NetworkAvatarSetup : NetworkBehaviour
{
    [SerializeField] GameObject m_ModeloHost;
    [SerializeField] GameObject m_ModeloCliente;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            bool soyHost = NetworkManager.Singleton.IsHost;
            if (m_ModeloHost != null) m_ModeloHost.SetActive(soyHost);
            if (m_ModeloCliente != null) m_ModeloCliente.SetActive(!soyHost);
        }
    }
}