using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 9f;
    private bool hasHit = false;

    private void OnEnable()
    {
        hasHit = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        CharacterLife playerLife = other.GetComponentInParent<CharacterLife>();

        if (playerLife != null)
        {
            playerLife.TakeDamage(damage);
            hasHit = true;
            Invoke(nameof(SetHasHitToFalse), 0.25f);

            Debug.Log("<color=orange>Hitbox enemigo golpeó al jugador</color>");
        }
    }

    public void SetHasHitToFalse()
    {
        hasHit = false;
    }
}
