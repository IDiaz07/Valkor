using UnityEngine;

public class GameResults : MonoBehaviour
{
    public static GameResults Instance;
    private GameResults gameResults;

    public bool didIWin = false;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
