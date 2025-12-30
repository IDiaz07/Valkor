using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Craftable;

public class PagesController : MonoBehaviour
{
    //TODO ¿Convertir Page en una clase para evitar repetir código?
    //Las páginas del libro de creación
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

    //Página 1
    private Image page1Image;
    private TMP_Text page1Title;
    private TMP_Text page1Description;
    private TMP_Text page1Requirements;
    private Button page1Button;

    //Página 2
    private Image page2Image;
    private TMP_Text page2Title;
    private TMP_Text page2Description;
    private TMP_Text page2Requirements;
    private Button page2Button;


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

        Item itemToAdd = newItem.GetComponent<Item>();
        if (itemToAdd == null) return;

        playerInventory.AddItem(
            newItem,
            itemToAdd.description,
            itemToAdd.itemID,
            itemToAdd.type,
            itemToAdd.icon
        );
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

        Item itemToAdd = newItem.GetComponent<Item>();
        if (itemToAdd == null) return;

        playerInventory.AddItem(
            newItem,
            itemToAdd.description,
            itemToAdd.itemID,
            itemToAdd.type,
            itemToAdd.icon
        );
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
            page2Image.sprite = currentPage2Craftable.craftable.GetComponent<Item>().icon;
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
        page2Description.text = currentPage2Craftable.craftable.GetComponent<Item>().description;
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
        page1Image.sprite = currentPage1Craftable.craftable.GetComponent<Item>().icon;
        page1Title.text = currentPage1Craftable.craftable.name;
        page1Description.text = currentPage1Craftable.craftable.GetComponent<Item>().description;
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
} 
