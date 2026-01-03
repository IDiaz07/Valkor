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

    void Start()
    {
        allSlots = slotHolder.transform.childCount;
        slot = new GameObject[allSlots];

        for (int i = 0; i < allSlots; i++)
        {
            slot[i] = slotHolder.transform.GetChild(i).gameObject;

            Slot s = slot[i].GetComponent<Slot>();
            if (s.Item == null)
                s.empty = true;
        }
    }

    void Update()
    {
        if (thumbstickA.action.WasPressedThisFrame())
            inventoryEnabled = !inventoryEnabled;

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
}
