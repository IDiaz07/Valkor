using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Craftable;

public class PagesController : MonoBehaviour
{
    //Las páginas del libro de creación
    [Header("Páginas",order = 0)]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

        GameObject newItem = Instantiate(currentPage1Craftable.craftable);

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
            page1Image.sprite = currentPage1Craftable.craftable.GetComponent<Item>().icon;
            page1Title.text = currentPage1Craftable.craftable.name;
            page1Description.text = currentPage1Craftable.craftable.GetComponent<Item>().description;
            page1Requirements.text = "";
            foreach (Requirement requirement in currentPage1Craftable.requirements)
            {
                page1Requirements.text += "" + requirement.amount + "x " + requirement.requirement.name + "\n";
            }
            //TODO Añadir código para que el botón se desactive si no hay suficientes recursos en el inventario cuando el inventario funcione
        }

        
    }

}
