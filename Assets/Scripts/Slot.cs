using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{


    public GameObject Item;
    public int ID = 0;
    public string type;
    public Sprite icon;
    [SerializeField] private Sprite defaultSprite;
    public string description;
    private int amount;
    public TextMeshProUGUI amountText;

    public bool empty;

    public Image image;

    public int Amount { get => amount; set { amount = value; UpdateUI(); } }

    void Start()
    {
        image = GetComponent<Image>();
        empty = true;
    }

    public void AddItem(GameObject itemToAdd, string desc, int id, string t, Sprite ic)
    {
        if (id == this.ID)
        {
            this.amount += 1;
            UpdateUI();
            return;
        }
        Item = itemToAdd;
        description = desc;
        ID = id;
        type = t;
        icon = ic;
        empty = false;
        amount = 1;

        image.sprite = icon;
        image.enabled = true;
        UpdateUI();
    }


    public void ClearSlot()
    {
        Item = null;
        description = "";
        ID = 0;
        type = "";
        icon = null;
        empty = true;
        image.sprite = defaultSprite;
        amount = 0;
    }

    public void UpdateUI()
    {
        if (amount == 0)
        {
            amountText.text = "";
            return;
        }
        amountText.text = amount.ToString();
    }
}
