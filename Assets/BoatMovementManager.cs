using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoatMovementManager : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable stearingHandle;
    [SerializeField] private GameObject openSails;
    [SerializeField] private GameObject closedSails;
    public bool isSailing;


    public void ManageSails()
    {
        if (isSailing)
        {
            openSails.SetActive(false);
            closedSails.SetActive(true);
            isSailing = false;
        }
        else
        {
            openSails.SetActive(true);
            closedSails.SetActive(false);
            isSailing=true;
        }
    }
}
