using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketProcessItem : MonoBehaviour
{
    [SerializeField] private GameObject processedItem;
    private XRSocketInteractor socketInteractor;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        socketInteractor = this.gameObject.GetComponent<XRSocketInteractor>();
    }



    public void ProcessItem()
    {
        Invoke(nameof(SwapItem), 5);
    }
    public void SwapItem()
    {
        Destroy(socketInteractor.firstInteractableSelected.transform.gameObject);
        GameObject newItem = Instantiate(processedItem);
        newItem.transform.position = this.transform.position;
    }
}
