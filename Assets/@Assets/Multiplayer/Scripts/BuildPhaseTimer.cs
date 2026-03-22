using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class BuildPhaseTimer : NetworkBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI buildMessageTMP;
    public TextMeshProUGUI timerTMP;

    [Header("Settings")]
    public float buildTime = 30f;

    private NetworkVariable<float> timeLeft = new NetworkVariable<float>(0f);

    private bool phaseEnded = false;

    public override void OnNetworkSpawn()
    {
        timeLeft.OnValueChanged += OnTimeChanged;

        if (IsServer)
        {
            StartCoroutine(WaitForPlayers());
        }
    }

    IEnumerator WaitForPlayers()
    {
        while (NetworkManager.Singleton.ConnectedClientsList.Count < 2)
        {
            yield return null;
        }

        StartCoroutine(StartBuildPhase());
    }

    IEnumerator StartBuildPhase()
    {
        ShowBuildMessageClientRpc();

        yield return new WaitForSeconds(3f);

        timeLeft.Value = buildTime;

        while (timeLeft.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft.Value -= 1f;
        }

        phaseEnded = true;
        EndBuildPhaseClientRpc();
    }

    void OnTimeChanged(float oldValue, float newValue)
    {
        if (phaseEnded) return;

        timerTMP.text = Mathf.CeilToInt(newValue).ToString();
    }

    [Rpc(SendTo.Everyone)]
    void ShowBuildMessageClientRpc()
    {
        StartCoroutine(ShowMessage());
    }

    IEnumerator ShowMessage()
    {
        buildMessageTMP.gameObject.SetActive(true);
        timerTMP.gameObject.SetActive(false);

        buildMessageTMP.text = "Construye tu base";

        yield return new WaitForSeconds(3f);

        buildMessageTMP.gameObject.SetActive(false);
        timerTMP.gameObject.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    void EndBuildPhaseClientRpc()
    {
        StartCoroutine(ShowFinalMessage());
    }

    IEnumerator ShowFinalMessage()
    {
        // Mostrar mensaje final
        timerTMP.gameObject.SetActive(true);
        timerTMP.text = "¡Atrapa la bandera!";

        yield return new WaitForSeconds(3f);


       
    }
}