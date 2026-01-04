using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicialVR : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene(1);
    }

    public void Salir()
    {
        Application.Quit();
    }
}

