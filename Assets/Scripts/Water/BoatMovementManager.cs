using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoatMovementManager : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable stearingHandle;
    [SerializeField] private GameObject openSails;
    [SerializeField] private GameObject closedSails;
    private Rigidbody rb;
    public bool isSailing;
    [SerializeField] private float boatSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    public void ManageSails()
    {
        if (isSailing)
        {
            CloseSails();
            isSailing = false;
        }
        else
        {
            OpenSails();
            isSailing=true;
        }
    }
    public void OpenSails()
    {
        openSails.SetActive(true);
        closedSails.SetActive(false);
    }

    public void CloseSails()
    {
        openSails.SetActive(false);
        closedSails.SetActive(true);
    }
    private void Update()
    {
        if (isSailing)
        {
            rb.AddForceAtPosition(openSails.transform.right*boatSpeed,openSails.transform.position, ForceMode.Force);
        }

    }
}
