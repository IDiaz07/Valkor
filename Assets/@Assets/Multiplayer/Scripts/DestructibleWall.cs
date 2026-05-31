using UnityEngine;
using Unity.Netcode;

public class DestructibleWall : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 5;

    // ── AUDIO ──────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioClip hitClip;       // Sonido al recibir golpe
    [SerializeField] private AudioClip destroyClip;   // Sonido al ser destruida
    private AudioSource audioSource;
    // ───────────────────────────────────────────────────────

    private NetworkVariable<int> health = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[WALL] Spawned | IsServer: {IsServer} | Owner: {OwnerClientId}");

        audioSource = GetComponent<AudioSource>();

        if (IsServer)
        {
            health.Value = maxHealth;
            Debug.Log($"[WALL][SERVER] Inicializando vida: {health.Value}");
        }

        health.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Debug.Log($"[WALL] Vida cambiada: {oldValue} -> {newValue}");

        // Suena en todos los clientes cuando la vida baja (pero la pared sigue viva)
        if (newValue < oldValue && newValue > 0)
        {
            if (audioSource != null && hitClip != null)
                audioSource.PlayOneShot(hitClip);
        }
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"[WALL] TakeDamage llamado | Cliente: {NetworkManager.Singleton.LocalClientId}");

        if (!IsSpawned)
        {
            Debug.LogError("[WALL] ❌ NO está spawneado en red");
            return;
        }

        if (health.Value <= 0)
        {
            Debug.Log("[WALL] Ya está destruida (cliente)");
            return;
        }

        DamageServerRpc(amount);
    }

    [Rpc(SendTo.Server)]
    private void DamageServerRpc(int amount)
    {
        if (!IsServer)
        {
            Debug.LogError("[WALL] ❌ Este código debería ejecutarse SOLO en el servidor");
            return;
        }

        if (!IsSpawned)
        {
            Debug.LogError("[WALL][SERVER] ❌ No está spawneado");
            return;
        }

        if (health.Value <= 0)
        {
            Debug.Log("[WALL][SERVER] Ya destruida");
            return;
        }

        Debug.Log($"[WALL][SERVER RPC] Daño recibido: {amount} | Vida actual: {health.Value}");

        health.Value -= amount;

        Debug.Log($"[WALL][SERVER] Nueva vida: {health.Value}");

        if (health.Value <= 0)
        {
            Debug.Log("[WALL][SERVER] 💥 Destruyendo pared");
            // Avisamos a todos para que suenen el audio de destrucción ANTES del Despawn
            PlayDestroyAudioRpc();
        }
    }

    // Se ejecuta en TODOS los clientes: reproduce el sonido de destrucción
    [Rpc(SendTo.Everyone)]
    private void PlayDestroyAudioRpc()
    {
        if (audioSource != null && destroyClip != null)
            audioSource.PlayOneShot(destroyClip);

        // Solo el servidor hace el despawn, con un pequeño delay para que el audio suene
        if (IsServer)
            Invoke(nameof(DestroyWall), 0.15f);
    }

    private void DestroyWall()
    {
        if (!IsServer)
        {
            Debug.LogError("[WALL] ❌ DestroyWall llamado fuera del servidor");
            return;
        }

        Debug.Log("[WALL][SERVER] Despawn()");
        GetComponent<NetworkObject>().Despawn(true);
    }

    public bool IsAlive()
    {
        return health.Value > 0;
    }

    public int GetHealth()
    {
        return health.Value;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
