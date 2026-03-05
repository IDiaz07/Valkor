using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private float moveSpeed = 3f;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    



    void Update()
    {
        if (!IsOwner) return;



    }

}
