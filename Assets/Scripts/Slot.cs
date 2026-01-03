
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Slot : MonoBehaviour
{
    public GameObject Item;
    public int ID = 0;
    public string type;
    public Sprite icon;

    [SerializeField] private Sprite defaultSprite;

    public string description;
    public int amount;
    public TextMeshProUGUI amountText;
    public bool empty = true;
    public Image image;

    private bool isProcessing = false;

    void Start()
    {
        image = GetComponent<Image>();
        empty = true;
        image.sprite = defaultSprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessing) return;
        if (!other.CompareTag("Item")) return;

        WorldItem worldItem = other.GetComponent<WorldItem>();
        if (worldItem == null) return;

        Item item = worldItem.itemData;

        if (!empty && ID != item.itemID) return;

        isProcessing = true;

        AddItem(
            other.gameObject,
            item.description,
            item.itemID,
            item.type,
            item.icon
        );

        Destroy(other.gameObject);
        isProcessing = false;
    }

    public void AddItem(GameObject itemToAdd, string desc, int id, string t, Sprite ic)
    {
        if (!empty && ID == id)
        {
            amount++;
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
        UpdateUI();
    }


    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (empty || amount <= 0) return;

        Transform hand = args.interactorObject.transform;

        RemoveOneItem(hand);
    }

    public void RemoveOneItem(Transform hand)
    {
        if (empty || amount <= 0) return;

        amount--;

        GameObject obj = Instantiate(
            Item.GetComponent<WorldItem>().itemData.worldPrefab
        );

        obj.transform.position = hand.position;
        obj.transform.rotation = hand.rotation;

        if (amount <= 0)
            ClearSlot();
        else
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
        UpdateUI();
    }

    private void UpdateUI()
    {
        amountText.text = amount > 1 ? amount.ToString() : "";
    }
}

