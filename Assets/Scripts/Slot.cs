using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{


    public GameObject Item;
    public int ID = 0;
    public string type;
    public Sprite icon;
    public string description;
    public int amount;
    public TextMeshProUGUI amountText;

    public bool empty;

    public Image image;


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
        image.sprite = null;
        image.enabled = false;
        amount = 0;
    }

    public void UpdateUI()
    {
        Debug.Log("update");
        amountText.text = amount.ToString();
    }
}
