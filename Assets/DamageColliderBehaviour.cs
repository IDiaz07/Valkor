using UnityEngine;

public class DamageColliderBehaviour : MonoBehaviour
{
    public int damage = 20;
    public Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colliding");
    }
}
