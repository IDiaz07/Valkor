using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    public InputActionProperty thumbstickA;
    CharacterController controller;

    public Transform rightHand;

    private int allSlots;
    private bool inventoryEnabled;
    public GameObject inventory;
    [SerializeField]
    private GameObject[] slot;
    public GameObject slotHolder;
    public bool isSailing;
    // Update is called once per frame
    void Start()
    {
        allSlots = slotHolder.transform.childCount;

        slot = new GameObject[allSlots];

        for (int i = 0; i < allSlots; i++) {


            slot[i]= slotHolder.transform.GetChild(i).gameObject;

            if (slot[i].GetComponent<Slot>().Item!=null)
            {
                slot[i].GetComponent<Slot>().empty = true;
            }
        }


    }

    private void Update()
    {
        if ( thumbstickA.action.WasPressedThisFrame())
        {
            inventoryEnabled = !inventoryEnabled;

        }
        if (inventoryEnabled)
        {
            inventory.SetActive(true);


            inventory.transform.SetParent(rightHand);

                
            inventory.transform.localPosition = new Vector3(0.1f, 0f, 0.2f);
            inventory.transform.localRotation = Quaternion.Euler(45, 0, 0);
        }
        else
        {
            inventory.SetActive(false); 
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
        GameObject pickedItem = other.gameObject;
           
        Item item = pickedItem.GetComponent<Item>();
         
            AddItem(pickedItem, item.description, item.itemID, item.type, item.icon);


        } 
}



    public void AddItem(GameObject itemToAdd, string description, int id, string type, Sprite icon)
    {
        for (int i = 0; i < allSlots; i++)
        {
            if (slot[i].transform.childCount == 0)
            {
                itemToAdd.GetComponent<Item>().pickedUp = true;

                slot[i].GetComponent<Slot>().AddItem(itemToAdd, description, id, type, icon);

                itemToAdd.transform.SetParent(slot[i].transform);
                itemToAdd.SetActive(false);
                slot[i].GetComponent<Slot>().empty = false;


            }
        }
    }
}
