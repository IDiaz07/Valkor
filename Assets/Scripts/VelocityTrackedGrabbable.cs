using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class VelocityTrackedGrabbable : XRGrabInteractable
{
    private Rigidbody rigidbody;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        base.Awake();
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        rigidbody.automaticCenterOfMass = false;
        SetParentToXRRig();
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        rigidbody.automaticCenterOfMass = true;
        SetParentToWorld();
        base.OnSelectExited(args);
    }

    public void SetParentToXRRig()
    {
        transform.SetParent(firstInteractorSelecting.transform);
    }

    public void SetParentToWorld()
    {
        transform.SetParent(null);
    }
}