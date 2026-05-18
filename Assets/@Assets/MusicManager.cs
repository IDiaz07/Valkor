using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicaMenu;
    public AudioSource musicaJuego;

    // Llama esta función al pulsar el botón de "Jugar"
    public void CambiarMusica()
    {
        musicaMenu.Stop();
        musicaJuego.Play();
    }
}