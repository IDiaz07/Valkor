using UnityEngine;

public class XRTriggerTeleport : MonoBehaviour
{
    public Transform xrOrigin;   // Arrastra aquí el XR Origin
    public Transform destino;    // Punto exacto del mapa

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Desactivar CharacterController antes de mover
            CharacterController cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Teletransporte EXACTO
            xrOrigin.position = destino.position;
            xrOrigin.rotation = destino.rotation;

            if (cc != null) cc.enabled = true;
        }
    }
}
