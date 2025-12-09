using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string description;
    public int itemID;
    public string type;
    public Sprite icon;
    public bool pickedUp;

    public Item(string description, int itemID, string type, Sprite icon, bool pickedUp)
    {
        this.description = description;
        this.itemID = itemID;
        this.type = type;
        this.icon = icon;
        this.pickedUp = pickedUp;
    }
}
