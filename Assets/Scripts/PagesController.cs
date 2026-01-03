using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static Craftable;

public class PagesController : MonoBehaviour
{

    [SerializeField] private bool isBuildingMenu = false;

    [Header("Páginas", order = 0)]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    public int curPageNum = 0;
    public int maxPageNum;

    public Craftable[] craftables;

    //Inventario
    [SerializeField] private Inventory playerInventory;

    //El crafteable activo de cada página
    private Craftable currentPage1Craftable;
    private Craftable currentPage2Craftable;

    [Header("Página 1")]
    //Página 1
    private Image page1Image;
    private TMP_Text page1Title;
    private TMP_Text page1Description;
    private TMP_Text page1Requirements;
    private Button page1Button;

    [Header("Página 2")]
    //Página 2
    private Image page2Image;
    private TMP_Text page2Title;
    private TMP_Text page2Description;
    private TMP_Text page2Requirements;
    private Button page2Button;

    [Header("Building menu")]
    private BuildingManager buildingManager;


    private void Awake()
    {
        page1Image = page1.transform.GetChild(0).GetComponent<Image>();
        page1Title = page1.transform.GetChild(1).GetComponent<TMP_Text>();
        page1Description = page1.transform.GetChild(2).GetComponent<TMP_Text>();
        page1Requirements = page1.transform.GetChild(3).GetComponent<TMP_Text>();
        page1Button = page1.transform.GetChild(4).GetComponent<Button>();


        page2Image = page2.transform.GetChild(0).GetComponent<Image>();
        page2Title = page2.transform.GetChild(1).GetComponent<TMP_Text>();
        page2Description = page2.transform.GetChild(2).GetComponent<TMP_Text>();
        page2Requirements = page2.transform.GetChild(3).GetComponent<TMP_Text>();
        page2Button = page2.transform.GetChild(4).GetComponent<Button>();

        if (isBuildingMenu) buildingManager = FindAnyObjectByType<BuildingManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckRequirementsAllPages();
    }

    public void SetPage1Craftable(Craftable craftable)
    {
        currentPage1Craftable = craftable;
    }
    public void SetPage2Craftable(Craftable craftable)
    {
        currentPage2Craftable = craftable;
    }

    public void CraftPage1Object()
    {
        if (currentPage1Craftable == null) return;
        if (currentPage1Craftable.craftable == null) return;


        foreach (Requirement requirement in currentPage1Craftable.requirements)
        {
            playerInventory.RemoveItemFromInventory(requirement.itemID, requirement.amount);
        }
        GameObject newItem = Instantiate(currentPage1Craftable.craftable);
        Debug.Log(newItem.GetInstanceID());

        Item itemToAdd = newItem.GetComponent<WorldItem>().itemData;
        if (itemToAdd == null) return;

       
    }

    public void CraftPage2Object()
    {
        if (currentPage2Craftable == null) return;
        if (currentPage2Craftable.craftable == null) return;


        foreach (Requirement requirement in currentPage2Craftable.requirements)
        {
            playerInventory.RemoveItemFromInventory(requirement.itemID, requirement.amount);
        }
        GameObject newItem = Instantiate(currentPage2Craftable.craftable);
        Debug.Log(newItem.GetInstanceID());

        Item itemToAdd = newItem.GetComponent<WorldItem>().itemData;
        if (itemToAdd == null) return;

        
    }

    private void OnEnable()
    {
        if (currentPage1Craftable != null)
        {
            //Página 1
            UpdatePage1UI();

        }
        if (currentPage2Craftable != null)
        {
            //Página 2
            UpdatePage2UI();

        }
    }

    public void UpdatePage2UI()
    {
        try
        {
            page2Image.sprite = currentPage2Craftable.GetComponent<WorldItem>().itemData.icon;
        }
        catch
        {
            page2Title.text = "";
            page2Description.text = "";
            page2Requirements.text = "";
            return;
        }
        page2Image.enabled = true;
        page2Title.text = currentPage2Craftable.craftable.name;
        page2Description.text = currentPage2Craftable.GetComponent<WorldItem>().itemData.description;
        page2Requirements.text = "";
        foreach (Requirement requirement in currentPage2Craftable.requirements)
        {
            page2Requirements.text += "" + requirement.amount + "x " + requirement.requirement.name + "\n";
        }
        page2Button.gameObject.SetActive(true);
        if (CheckMeetsRequirements(currentPage2Craftable.requirements))
        {
            page2Button.interactable = true;
        }
        else
        {
            page2Button.interactable = false;
        }
    }

    public void UpdatePage1UI()
    {
        page1Image.sprite = currentPage1Craftable.GetComponent<WorldItem>().itemData.icon;
        page1Title.text = currentPage1Craftable.name;
        page1Description.text = currentPage1Craftable.GetComponent<WorldItem>().itemData.description;
        page1Requirements.text = "";
        foreach (Requirement requirement in currentPage1Craftable.requirements)
        {
            page1Requirements.text += "" + requirement.amount + "x " + requirement.requirement.name + "\n";
        }
        if (CheckMeetsRequirements(currentPage1Craftable.requirements))
        {
            page1Button.interactable = true;
        }
        else
        {
            page1Button.interactable = false;
        }
    }

    private bool CheckMeetsRequirements(Requirement[] requirementList)
    {
        foreach (Requirement requirement in requirementList)
        {
            if (!playerInventory.FindObjectInInventory(requirement.itemID, requirement.amount))
            {
                return false;
            }
        }
        return true;
    }

    private void CheckRequirementsAllPages()
    {
        if (currentPage1Craftable != null && CheckMeetsRequirements(currentPage1Craftable.requirements))
        {
            page1Button.interactable = true;
        }
        else
        {
            page1Button.interactable = false;
        }
        if (currentPage2Craftable != null && CheckMeetsRequirements(currentPage2Craftable.requirements))
        {
            page2Button.interactable = true;
        }
        else
        {
            page2Button.interactable = false;
        }
    }

    public void EmptyPage2()
    {
        currentPage2Craftable = null;
        page2Image.sprite = null;
        page2Image.enabled = false;
        page2Title.text = "";
        page2Description.text = "";
        page2Button.gameObject.SetActive(false);
    }

    public void NextPage()
    {
        if (curPageNum < maxPageNum)
        {
            curPageNum += 1;
            SetPage1Craftable(craftables[0 + curPageNum * 2]);
            UpdatePage1UI();
            try
            {
                SetPage2Craftable(craftables[1 + curPageNum * 2]);
            }
            catch (System.Exception)
            {
                EmptyPage2();
            }
            UpdatePage2UI();
        }

    }
    public void PreviousPage()
    {
        if (curPageNum > 0)
        {
            curPageNum -= 1;
            SetPage1Craftable(craftables[0 + curPageNum * 2]);
            UpdatePage1UI();
            SetPage2Craftable(craftables[1 + curPageNum * 2]);
            UpdatePage2UI();
        }
    }
    public void BuildObjectPage1()
    {
        ChangeBuildingState();
        buildingManager.ChangeSelectedBuildType(currentPage1Craftable.craftable.gameObject.GetComponent<Buildable>().Type);
        buildingManager.ChargeCurrentBuildIndex(currentPage1Craftable.craftable.gameObject.GetComponent<Buildable>().BuildId);

    }
    public void BuildObjectPage2()
    {
        ChangeBuildingState();
        buildingManager.ChangeSelectedBuildType(currentPage2Craftable.craftable.gameObject.GetComponent<Buildable>().Type);
        buildingManager.ChargeCurrentBuildIndex(currentPage2Craftable.craftable.gameObject.GetComponent<Buildable>().BuildId);

    }
    public void ChangeBuildingState()
    {
        if (isBuildingMenu)
            buildingManager.ChangeBuildingState();
        buildingManager.RaycastObject = buildingManager.GetOppositeHand(this.transform.root.GetComponent<XRGrabInteractable>().interactorsSelecting[0].transform.parent);
    }

    // Te permite activar el modo desstrucción si está desactivado y viceversa.
    public void ChangeDestructionState()
    {
        if(isBuildingMenu)
        buildingManager.ChangeDestructionState();
    }
} 
