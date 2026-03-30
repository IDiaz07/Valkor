using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchEndHandler : NetworkBehaviour
{
    // The server calls this when the game ends
    [Rpc(SendTo.Everyone)]
    public void TriggerGameOverRpc(ulong winnerClientId)
    {
        Debug.Log("¡Hemos llegado a match handler!");
        bool didIWin = (NetworkManager.Singleton.LocalClientId == winnerClientId);
        GameResults.Instance.didIWin = didIWin;

        // 2. Shut down the network session
        NetworkManager.Singleton.Shutdown();

        // 3. Load the offline scene natively
        SceneManager.LoadScene("GameOverScene");
    }
}