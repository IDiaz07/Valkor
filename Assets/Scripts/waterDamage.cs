using UnityEngine;

public class WaterDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 15f;

    private void OnTriggerStay(Collider other)
    {
        CharacterLife playerLife = other.GetComponentInParent<CharacterLife>();

        if (playerLife != null)
        {
            playerLife.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}