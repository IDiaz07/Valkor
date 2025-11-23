using Unity.VisualScripting;
using UnityEngine;

public class simpleBuoyancyScript : MonoBehaviour
{

    [SerializeField]
    private float buoyancyReduction = 0.01f;
    [SerializeField]
    private float dampingReduction= 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        try
        {
            Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
            //La profundidad a la que está sumergido el objeto
            float depth = (this.gameObject.transform.position.y - rb.transform.TransformPoint(rb.centerOfMass).y);
            if (depth > 0)
            {
                rb.AddForce(this.transform.up * (other.bounds.size.magnitude / buoyancyReduction * depth), ForceMode.Force);

                Vector3 tilt = Vector3.Cross(rb.transform.up, Vector3.up);
                rb.AddTorque(tilt * 10f, ForceMode.Force);

                rb.AddTorque(-rb.angularVelocity * 2f, ForceMode.Force);


                //Amortiguación para reducir el efecto rebote
                float damping = -rb.linearVelocity.y *1/dampingReduction;
                rb.AddForce(transform.up * damping, ForceMode.Force);
            }
        }
        catch (System.Exception e)
        {
        }
    }
}
