using UnityEngine;

public class CraftingBookController : MonoBehaviour
{
    [SerializeField]
    private GameObject bookInterface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableInterface()
    {
        bookInterface.SetActive(true);
    }
    public void DisableInterface()
    {
        bookInterface.SetActive(false);
    }
}
