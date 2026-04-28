using UnityEngine;
using Unity.Netcode;

public class DestructibleWall : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 5;

    private NetworkVariable<int> health = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[WALL] Spawned | IsServer: {IsServer} | Owner: {OwnerClientId}");

        if (IsServer)
        {
            health.Value = maxHealth;
            Debug.Log($"[WALL][SERVER] Inicializando vida: {health.Value}");
        }

        health.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Debug.Log($"[WALL] Vida cambiada: {oldValue} -> {newValue}");
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"[WALL] TakeDamage llamado | Cliente: {NetworkManager.Singleton.LocalClientId}");

        if (!IsSpawned)
        {
            Debug.LogError("[WALL] NO está spawneado en red");
            return;
        }

        DamageServerRpc(amount);
    }

    [Rpc(SendTo.Server)]
    private void DamageServerRpc(int amount)
    {
        Debug.Log($"[WALL][SERVER RPC] Recibido daño: {amount} | Vida actual: {health.Value}");

        if (!IsServer)
        {
            Debug.LogError("[WALL] Este código debería ejecutarse SOLO en el servidor");
            return;
        }

        if (health.Value <= 0)
        {
            Debug.Log("[WALL][SERVER] Ya está destruida");
            return;
        }

        health.Value -= amount;

        Debug.Log($"[WALL][SERVER] Nueva vida: {health.Value}");

        if (health.Value <= 0)
        {
            Debug.Log("[WALL][SERVER] Destruyendo pared");
            DestroyWall();
        }
    }

    private void DestroyWall()
    {
        if (!IsServer)
        {
            Debug.LogError("[WALL] DestroyWall llamado fuera del servidor");
            return;
        }

        Debug.Log("[WALL][SERVER] Despawn()");
        GetComponent<NetworkObject>().Despawn(true);
    }
}