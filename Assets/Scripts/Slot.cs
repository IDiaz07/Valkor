using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{


    public GameObject Item;
    public int ID = 0;
    public string type;
    public Sprite icon;
    public string description;

    public bool empty;

    public Image image;
    

    void Start()
    {
        image = GetComponent<Image>();
        empty = true;
    }

    public void AddItem(GameObject itemToAdd, string desc, int id, string t, Sprite ic)
    {
        Item = itemToAdd;
        description = desc;
        ID = id;
        type = t;
        icon = ic;
        empty = false;

        image.sprite = icon;
        image.enabled = true;
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
    }
}
