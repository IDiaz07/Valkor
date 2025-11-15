using System;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
{
    [SerializeField]
    private int treeHealth = 100;
    private AudioSource treeSound;
    private ParticleSystem treeDust;
    [SerializeField]
    private GameObject woodDrop;
    private void Awake()
    {
        treeSound = this.gameObject.GetComponent<AudioSource>();
        treeDust = this.gameObject.GetComponent<ParticleSystem>();
    }
    public void LoseHealth(int healthLoss)
    {
        treeHealth -= healthLoss;
        if(treeHealth <= 0)
        {
            Fall();
        }
    }

    private void Fall()
    {
        //TODO a�adir particulas, sonido, tronco, etc
        Instantiate(woodDrop);
        Destroy(this.gameObject, 0.06f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colliding with something");
        if (collision.collider.gameObject.CompareTag("DamageCollider"))
        {
            Debug.Log("Colliding with weapon");
            DamageColliderBehaviour damagerBehaviour = collision.collider.gameObject.GetComponent<DamageColliderBehaviour>();
            if (damagerBehaviour.rigidbody.linearVelocity.magnitude > 2)
            {
                LoseHealth(damagerBehaviour.damage);
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = collision.GetContact(0).point;
                treeDust.Emit(emitParams, 30);
                treeSound.Play();
            }
        }
    }
}
