using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[System.Serializable]
public class AnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}

public class AnimateOnInput : NetworkBehaviour
{
    public List<AnimationInput> animationInputs;
    public Animator animator;

    void Update()
    {
        if (!IsOwner) return; // solo el dueño lee el input

        foreach (var item in animationInputs)
        {
            float actionValue = item.action.action.ReadValue<float>();
            animator.SetFloat(item.animationPropertyName, actionValue);
            UpdateAnimationServerRpc(item.animationPropertyName, actionValue);
        }
    }

    [ServerRpc]
    private void UpdateAnimationServerRpc(string propertyName, float value)
    {
        UpdateAnimationClientRpc(propertyName, value);
    }

    [ClientRpc]
    private void UpdateAnimationClientRpc(string propertyName, float value)
    {
        if (IsOwner) return; // el dueño ya lo tiene aplicado
        animator.SetFloat(propertyName, value);
    }
}