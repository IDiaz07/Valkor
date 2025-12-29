using UnityEngine;
using UnityEngine.UIElements;

public class CraftingBookController : MonoBehaviour
{
    [SerializeField]
    private GameObject bookInterface;
    [SerializeField]
    private Craftable[] craftables;

    private PagesController pagesController;



    private void Awake()
    {
        pagesController = bookInterface.GetComponent<PagesController>();
        if (craftables.Length > 0)
            pagesController.SetPage1Craftable(craftables[0]);
        if (craftables.Length >1)
            pagesController.SetPage2Craftable(craftables[1]);
    }



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

