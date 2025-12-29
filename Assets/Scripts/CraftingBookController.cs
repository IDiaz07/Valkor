using UnityEngine;
using UnityEngine.UIElements;

public class CraftingBookController : MonoBehaviour
{
    [SerializeField]
    private GameObject bookInterface;
    [SerializeField]
    private Craftable[] craftables;

    private PagesController pagesController;

    private int curPageNum = 0;
    private int maxPageNum;



    private void Awake()
    {
        maxPageNum = (craftables.Length - 1) / 2;
        pagesController = bookInterface.GetComponent<PagesController>();
            
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pagesController.SetPage1Craftable(craftables[0 + curPageNum * 2]);
        pagesController.SetPage2Craftable(craftables[1 + curPageNum * 2]);
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

    public void NextPage()
    {
        if (curPageNum < maxPageNum)
        {
            curPageNum += 1;
            pagesController.SetPage1Craftable(craftables[0 + curPageNum * 2]);
            pagesController.UpdatePage1UI();
            try
            {
                pagesController.SetPage2Craftable(craftables[1 + curPageNum * 2]);
            }
            catch (System.Exception)
            {
                pagesController.EmptyPage2();
            }
            pagesController.UpdatePage2UI();
        }
        
    }
    public void PreviousPage()
    {
        if (curPageNum > 0)
        {
            curPageNum -= 1;
            pagesController.SetPage1Craftable(craftables[0 + curPageNum * 2]);
            pagesController.UpdatePage1UI();
            pagesController.SetPage2Craftable(craftables[1 + curPageNum * 2]);
            pagesController.UpdatePage2UI();
        }
    }
}

