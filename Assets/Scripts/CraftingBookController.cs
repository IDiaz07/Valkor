using UnityEngine;
using UnityEngine.UIElements;

public class CraftingBookController : MonoBehaviour
{
    [SerializeField]
    private GameObject bookInterface;
    [SerializeField]
    private GameObject mainMenuSection;
    [SerializeField]
    private GameObject craftSection;
    [SerializeField]
    private GameObject buildSection;
    [SerializeField]
    private Craftable[] craftables;
    [SerializeField]
    private Craftable[] buildables;

    private PagesController craftPagesController;
    private PagesController buildPagesController;

    private int curCraftPageNum = 0;
    private int maxCraftPageNum;

    private int curBuildPageNum = 0;
    private int maxBuildPageNum;



    private void Awake()
    {
        maxCraftPageNum = (craftables.Length - 1) / 2;
        maxBuildPageNum = (buildables.Length - 1) / 2;
        craftPagesController = craftSection.GetComponent<PagesController>();
        buildPagesController = buildSection.GetComponent<PagesController>();

        craftPagesController.curPageNum = curCraftPageNum;
        craftPagesController.maxPageNum = maxCraftPageNum;

        buildPagesController.curPageNum = curBuildPageNum;
        buildPagesController.maxPageNum = maxBuildPageNum;

    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        craftPagesController.SetPage1Craftable(craftables[0 + curCraftPageNum * 2]);
        craftPagesController.SetPage2Craftable(craftables[1 + curCraftPageNum * 2]);

        buildPagesController.SetPage1Craftable(buildables[0 + curBuildPageNum * 2]);
        buildPagesController.SetPage2Craftable(buildables[1 + curBuildPageNum * 2]);
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

    public void EnableCraftingInterface()
    {
        craftSection.SetActive(true);
        craftPagesController.craftables = craftables;
        mainMenuSection.SetActive(false);
    }
    public void DisableCraftingInterface()
    {
        mainMenuSection.SetActive(true);
        craftSection.SetActive(false);
    }

    public void EnableBuildingInterface()
    {
        buildSection.SetActive(true);
        buildPagesController.craftables = buildables;
        mainMenuSection.SetActive(false);


    }
    public void DisableBuildingInterface()
    {
        mainMenuSection.SetActive(true);
        buildSection.SetActive(false);
    }

    public void NextPage()
    {
        if (curCraftPageNum < maxCraftPageNum)
        {
            curCraftPageNum += 1;
            craftPagesController.SetPage1Craftable(craftables[0 + curCraftPageNum * 2]);
            craftPagesController.UpdatePage1UI();
            try
            {
                craftPagesController.SetPage2Craftable(craftables[1 + curCraftPageNum * 2]);
            }
            catch (System.Exception)
            {
                craftPagesController.EmptyPage2();
            }
            craftPagesController.UpdatePage2UI();
        }
        
    }
    public void PreviousPage()
    {
        if (curCraftPageNum > 0)
        {
            curCraftPageNum -= 1;
            craftPagesController.SetPage1Craftable(craftables[0 + curCraftPageNum * 2]);
            craftPagesController.UpdatePage1UI();
            craftPagesController.SetPage2Craftable(craftables[1 + curCraftPageNum * 2]);
            craftPagesController.UpdatePage2UI();
        }
    }
}

