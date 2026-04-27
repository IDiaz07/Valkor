using UnityEngine;

public class VRHandAttack : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldown = 0.5f;

    private float lastHitTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastHitTime < cooldown) return;

        DestructibleWall wall = collision.gameObject.GetComponent<DestructibleWall>();

        if (wall != null)
        {
            wall.TakeDamage(damage);
            lastHitTime = Time.time;
        }
    }
}