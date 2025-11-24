using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    public InputActionProperty thumbstickA;
    CharacterController controller;

    private int allSlots;
    private bool inventoryEnabled;
    public GameObject inventory;
    private GameObject[] slot;
    public GameObject slotHolder;
    // Update is called once per frame
    void Start()
    {
        allSlots = slotHolder.transform.childCount;

        slot = new GameObject[allSlots];

        for (int i = 0; i < allSlots; i++) {


            slot[i]= slotHolder.transform.GetChild(i).gameObject;
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
        }
        else
        {
            inventory.SetActive(false); 
        }
    }



}
