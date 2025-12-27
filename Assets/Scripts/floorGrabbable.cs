using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Item))]
public class floorGrabbable : MonoBehaviour
{
    private XRSimpleInteractable xRSimpleInteractable;
    float initialDistance = 0;
    float curDistance = 0;
    private Item item;

    private void Awake()
    {
        xRSimpleInteractable = this.gameObject.GetComponent<XRSimpleInteractable>();
        item = this.GetComponent<Item>();
    }


    private void Update()
    {
        if(initialDistance != 0)
        {
            curDistance = GetDistanceToInteractor();
            if (curDistance > initialDistance + 0.4)
            {
                AddToInventory();
            }
        }
    }

    private void AddToInventory()
    {
        Debug.Log("AddingToInventory");
        Inventory inventory = FindFirstObjectByType<Inventory>();
        inventory.AddItem(gameObject, item.description, item.itemID, item.type, item.icon);
        Destroy(this.gameObject);
    }

    public void XrPullMotionDetector()
    {
        initialDistance = GetDistanceToInteractor();
        curDistance = initialDistance;
    }

    public void XrStopPullMotionDetector()
    {
        initialDistance = 0;
        curDistance = 0;
    }

    private float GetDistanceToInteractor()
    {
        return Vector3.Distance(this.transform.position, xRSimpleInteractable.firstInteractorSelecting.transform.position);
    }
}
