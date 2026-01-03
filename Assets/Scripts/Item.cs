using UnityEngine;

[System.Serializable]
public class Item
{
    public string description;
    public int itemID;
    public string type;
    public Sprite icon;
    public GameObject worldPrefab;
    public string prefabName;

    public Item(string description, int itemID, string type, Sprite icon, GameObject prefab)
    {
        this.description = description;
        this.itemID = itemID;
        this.type = type;
        this.icon = icon;
        this.worldPrefab = prefab;
    }
}


