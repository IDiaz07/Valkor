using UnityEngine;

public class VRHandAttack : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[HAND] Entrando en: " + other.name);

        if (hasHit) return;
        hasHit = true;

        DestructibleWall wall = other.GetComponentInParent<DestructibleWall>();

        if (wall != null && wall.IsSpawned)
        {
            Debug.Log("[HAND] GOLPE ÚNICO");
            wall.TakeDamage(damage);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("[HAND] Saliendo de: " + other.name);

        // Cuando sales, puedes volver a golpear
        hasHit = false;
    }
}