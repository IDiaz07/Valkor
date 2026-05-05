using UnityEngine;
using System.Collections.Generic;

public class VRHandAttack : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private HashSet<DestructibleWall> hitWalls = new HashSet<DestructibleWall>();

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[HAND] Entrando en: " + other.name);

        DestructibleWall wall = other.GetComponentInParent<DestructibleWall>();

        if (wall != null && wall.IsSpawned)
        {
            if (hitWalls.Contains(wall))
                return;

            Debug.Log("[HAND] 💥 GOLPE ÚNICO");

            wall.TakeDamage(damage);

            hitWalls.Add(wall);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("[HAND] Saliendo de: " + other.name);

        DestructibleWall wall = other.GetComponentInParent<DestructibleWall>();

        if (wall != null)
        {
            if (hitWalls.Contains(wall))
            {
                hitWalls.Remove(wall);
                Debug.Log("[HAND] 🔄 Reset golpe para esa pared");
            }
        }
    }
}