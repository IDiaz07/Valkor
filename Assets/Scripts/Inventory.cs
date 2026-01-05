using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField] public InputActionProperty thumbstickA;

    public Transform rightHand;

    private int allSlots;
    private bool inventoryEnabled;

    public GameObject inventory;
    public GameObject slotHolder;

    private GameObject[] slot;
    public bool addingObject = false;

    void Start()
    {
        allSlots = slotHolder.transform.childCount;
        slot = new GameObject[allSlots];

        for (int i = 0; i < allSlots; i++)
        {
            slot[i] = slotHolder.transform.GetChild(i).gameObject;

            Slot s = slot[i].GetComponent<Slot>();
            if (s.item == null)
                s.empty = true;
        }
    }

    void Update()
    {
        if (thumbstickA.action.WasPressedThisFrame())
        {
            inventoryEnabled = !inventoryEnabled;
            //Por si la variable se quedase atascada en true por alguna razón, se resetea al abrir o cerrar el inventario
            addingObject = false;

        }

        inventory.SetActive(inventoryEnabled);

        if (inventoryEnabled)
        {
            inventory.transform.SetParent(rightHand);
            inventory.transform.localPosition = new Vector3(0.1f, 0f, 0.2f);
            inventory.transform.localRotation = Quaternion.Euler(45, 0, 0);
        }
    }




    public void RemoveItemFromInventory(int itemId, int amount)
    {
        for (int i = 0; i < allSlots; i++)
        {
            Slot s = slot[i].GetComponent<Slot>();

            if (!s.empty && s.ID == itemId)
            {
                s.amount -= amount;

                if (s.amount <= 0)
                    s.ClearSlot();

                return;
            }
        }
    }


    public bool FindObjectInInventory(int objectId, int minAmount)
    {
        for (int i = 0; i < allSlots; i++)
        {
            Slot s = slot[i].GetComponent<Slot>();

            if (!s.empty && s.ID == objectId && s.amount >= minAmount)
                return true;
        }
        return false;
    }
    //Como FindObjectInInventory, pero te da el idex del objeto y devuelve -1 si no está
    public int GetSlotIndex(int objectId)
    {
        for (int i = 0; i < allSlots; i++)
        {
            Slot s = slot[i].GetComponent<Slot>();

            if (!s.empty && s.ID == objectId)
                return i;
        }
        return -1;
    }
    public void AddItem(GameObject itemToAdd, string description, int id, string type, Sprite icon)
    {
        int objectSlotIndex = GetSlotIndex(id);
        if (objectSlotIndex != -1)
        {
            slot[objectSlotIndex].GetComponent<Slot>().AddItem(itemToAdd, description, id, type, icon);
        }
        else
        {
            for (int i = 0; i < allSlots; i++)
            {
                Slot s = slot[i].GetComponent<Slot>();

                if (s.empty)
                {
                    s.AddItem(itemToAdd, description, id, type, icon);
                    break;
                }
            }
        }
    }
}

