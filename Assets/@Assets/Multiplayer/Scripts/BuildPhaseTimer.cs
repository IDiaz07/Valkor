using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class BuildPhaseTimer : NetworkBehaviour
{
    public TextMeshProUGUI buildMessageTMP;
    public TextMeshProUGUI timerTMP;

    public int buildTime = 30;

    private NetworkVariable<int> timeLeft = new NetworkVariable<int>();
    private float timer;
    private bool started = false;

    void Start()
    {
        buildMessageTMP.gameObject.SetActive(false);
        timerTMP.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        timeLeft.OnValueChanged += OnTimerChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
        }
    }

    void OnPlayerConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2 && !started)
        {
            started = true;
            StartCoroutine(StartBuildPhase());
        }
    }

    IEnumerator StartBuildPhase()
    {
        ShowMessageClientRpc();

        yield return new WaitForSeconds(3);

        timeLeft.Value = buildTime;
        StartTimerClientRpc();
    }

    void Update()
    {
        if (!IsServer) return;
        if (timeLeft.Value <= 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            timer = 0f;
            timeLeft.Value--;
        }
    }

    void OnTimerChanged(int oldValue, int newValue)
    {
        timerTMP.text = newValue.ToString();
    }

    [ClientRpc]
    void ShowMessageClientRpc()
    {
        buildMessageTMP.gameObject.SetActive(true);
        buildMessageTMP.text = "Construye tu base";
    }

    [ClientRpc]
    void StartTimerClientRpc()
    {
        buildMessageTMP.gameObject.SetActive(false);
        timerTMP.gameObject.SetActive(true);
    }
}