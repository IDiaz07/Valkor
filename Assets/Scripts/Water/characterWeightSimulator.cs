using UnityEngine;

public class characterWeightSimulator : MonoBehaviour
{
    [SerializeField]
    private float characterWeight;
    [SerializeField]
    private CharacterController characterController;
    private Collider characterCollider;

    void Awake()
    {
      characterController = GetComponent<CharacterController>();
        characterCollider = GetComponent<Collider>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision);
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForceAtPosition(Vector3.down * characterWeight, collision.collider.ClosestPoint(characterController.transform.position));
        }
        else { return;}
    }*/
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
