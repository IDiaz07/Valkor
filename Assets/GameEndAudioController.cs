using UnityEngine;

public class GameEndAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winAudio;
    [SerializeField] private AudioClip loseAudio;
    private GameResults results;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        results = FindAnyObjectByType<GameResults>();
    }
    private void Start()
    {
        if (results.didIWin)
        {
            audioSource.clip = winAudio;
        }
        else
        {
            audioSource.clip = loseAudio;
        }
        audioSource.Play();
    }

}
