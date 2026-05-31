using UnityEngine;

public class GameMusicManager : MonoBehaviour
{
    [Header("Pistas de música")]
    [SerializeField] private AudioClip musicaMenu;
    [SerializeField] private AudioClip musicaConstruccion;
    [SerializeField] private AudioClip musicaCombate;

    [Header("Volumen por pista (0 = silencio, 1 = máximo)")]
    [Range(0f, 1f)] [SerializeField] private float volumenMenu         = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float volumenConstruccion = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float volumenCombate      = 1f;

    public static GameMusicManager Instance;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        CambiarMusica(musicaMenu, volumenMenu);
    }

    // Llamado por BuildPhaseTimer cuando arranca el contador
    public void CambiarAConstruccion()
    {
        audioSource.Stop();
        CambiarMusica(musicaConstruccion, volumenConstruccion);
    }

    // Llamado por BuildPhaseTimer cuando termina la fase de construcción
    public void CambiarACombate()
    {
        CambiarMusica(musicaCombate, volumenCombate);
    }

    // Llama a este método desde cualquier sitio para parar la música
    public void PararMusica()
    {
        audioSource.Stop();
    }

    private void CambiarMusica(AudioClip nuevaMusica, float volumen)
    {
        if (nuevaMusica == null) return;
        audioSource.Stop();
        audioSource.clip = nuevaMusica;
        audioSource.volume = volumen;
        audioSource.Play();
    }
}
