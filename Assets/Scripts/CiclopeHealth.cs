using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CiclopeHealth : MonoBehaviour
{

    public float maxHealth = 100f;

    private float currentHealth;
    private bool isDead = false;

    private CiclopeAnimations animations;

    void Start()
    {

        currentHealth =
            maxHealth;

        animations =
            GetComponent<
                CiclopeAnimations
            >();

        // 👉 DO NOT force trigger on body
        GetComponent<
            Collider
        >().isTrigger =
            false;
    }

    public void TakeDamage(float damage)
    {

        if (isDead)
            return;

        currentHealth -=
            damage;

        Debug.Log(
            "Cyclops hit – health: "
            + currentHealth
        );

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {

        if (isDead)
            return;

        isDead = true;

        if (animations != null)
            animations.Die();

        Destroy(
            gameObject,
            5f
        );
    }

    void OnTriggerEnter(Collider other)
    {

        if (
            other.gameObject.layer ==
            LayerMask.NameToLayer(
                "Weapon"
            )
        )
        {

            TakeDamage(30f);
        }
    }
}
