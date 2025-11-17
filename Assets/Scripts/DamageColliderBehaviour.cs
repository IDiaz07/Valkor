using UnityEngine;

public class DamageColliderBehaviour : MonoBehaviour
{
    public int damage = 20;
    public Rigidbody rigidbody;

 

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colliding");
    }
}
