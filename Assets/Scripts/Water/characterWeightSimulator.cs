using UnityEngine;

public class characterWeightSimulator : MonoBehaviour
{
    [SerializeField]
    private float characterWeight;
    [SerializeField]
    private CharacterController characterController;
    private Collider characterCollider;
    private bool isOnBoat = false;
    private Transform boatTransform;
    private Vector3 boatPosition;
    private Vector3 boatDelta;

    void Awake()
    {
      characterController = GetComponent<CharacterController>();
        characterCollider = GetComponent<Collider>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnBoat)
        {
            boatDelta = boatTransform.position - boatPosition;
            this.transform.position += boatDelta;
            boatPosition = boatTransform.position;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if ( other.CompareTag("Boat"))
        {
            Debug.LogWarning("Subido al barco");
            GetOnBoat(other.gameObject.transform.root);
        }
    }

    private void GetOnBoat(Transform boat)
    {
        isOnBoat = true;
        boatTransform = boat;
        boatPosition = boatTransform.position;
    }
    private void GetOffBoat(Transform boat)
    {
        isOnBoat = false;
        boatTransform = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Boat"))
        {
            isOnBoat = false;
            Debug.LogWarning("Bajado del barco");
        }
    }
    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("collision");
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForceAtPosition(Vector3.down * characterWeight, other.ClosestPoint(characterController.transform.position));
            //Debug.Log("forceAdded");
        }
        //else { Debug.Log("Could not get rigidBody for " + other.gameObject.name); return; }
    }
}
