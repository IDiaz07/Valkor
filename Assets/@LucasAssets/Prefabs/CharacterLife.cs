using UnityEngine;

public class CharacterLife : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float actualHealth;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float actualStamina;

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float ActualHealth { get => actualHealth; set => actualHealth = value; }
    public float MaxStamina { get => maxStamina; set => maxStamina = value; }
    public float ActualStamina { get => actualStamina; set => actualStamina = value; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actualHealth = maxHealth;
        actualStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (actualHealth <= 0) Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Frog"))
        {
            actualHealth -= 9f;
            Debug.Log($"Daño de rana recivido. Vida actual ={actualHealth}");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Slime"))
        {
            actualHealth -= 5f;
            Debug.Log($"Daño de rana recivido. Vida actual ={actualHealth}");
        }
    }

    private void Die()
    {
        Debug.Log("MORISTE DEJA DE JUGAR");
    }
}
