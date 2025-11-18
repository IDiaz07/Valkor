using System;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private int treeHealth = 100;
    private AudioSource treeSound;
    [SerializeField]
    private GameObject treeCutEffectsSpawner;
    private ParticleSystem treeDust;
    [SerializeField]
    private GameObject woodDrop;
    private Rigidbody rigidbody;
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
        
        Invoke(nameof(SpawnWood), 3);
    }

    private void SpawnWood()
    {
        Instantiate(woodDrop,this.transform.position,this.transform.rotation);
        Instantiate(treeCutEffectsSpawner, this.transform.position, this.transform.rotation);
        if (parent != null)
        {
            Destroy(parent, 0.06f);
        }
        else
        {
            Destroy(this.gameObject, 0.06f);
        }
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
