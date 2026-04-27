using UnityEngine;
using Unity.Netcode;

public class DestructibleWall : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 5;

    private NetworkVariable<int> health = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }

        health.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Debug.Log("Vida pared: " + newValue);
    }

    public void TakeDamage(int amount)
    {
        // Cualquier cliente puede pedir daño → el servidor decide
        DamageServerRpc(amount);
    }

    [Rpc(SendTo.Server)]
    private void DamageServerRpc(int amount)
    {
        if (health.Value <= 0) return;

        health.Value -= amount;

        if (health.Value <= 0)
        {
            DestroyWall();
        }
    }

    private void DestroyWall()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}