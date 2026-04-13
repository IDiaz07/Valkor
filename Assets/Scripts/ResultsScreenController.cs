using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultsScreenController : MonoBehaviour
{
    private TMP_Text textBox;
    private GameResults results;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        results = FindAnyObjectByType<GameResults>();
        textBox = this.gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Start()
    {
        if (results.didIWin)
        {
            textBox.text = "Éxito: ¡Has Ganado!";
        }
        else
        {
            textBox.text = "GAME OVER: ¡Has Perdido!";
        }
    }
    public void RestartGame()
    {
        results = FindAnyObjectByType<GameResults>();
        Destroy(results);
        SceneManager.LoadScene("Test");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
