using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ParticleSystem))]
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
    [SerializeField]
    private int woodDropAmount = 1;
    private bool isFalling = false;

    private Rigidbody rigidbody;
    private void Awake()
    {
        treeSound = this.gameObject.GetComponent<AudioSource>();
        treeDust = this.gameObject.GetComponent<ParticleSystem>();
    }
    public void LoseHealth(int healthLoss)
    {
        treeHealth -= healthLoss;
        if(treeHealth <= 0 && !isFalling)
        {
            Fall();
        }
    }

    private void Fall()
    {
        isFalling = true;
        
        SpawnWood(woodDropAmount);
    }

    private void SpawnWood(int spawnQuantity)
    {
        for (int i = 0; i < spawnQuantity; i++)
        {
            Instantiate(woodDrop, this.transform.position + new Vector3(0,1+i*2,0), Quaternion.Euler(Vector3.up));
        }
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
