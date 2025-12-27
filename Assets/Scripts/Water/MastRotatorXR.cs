using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MastRotatorXR : MonoBehaviour
{
    public Rigidbody mastRigidbody;
    public XRGrabInteractable handle;
    public float torqueMultiplier = 5f;

    private Transform grabbingHand;
    private bool isGrabbed = false;

    private void OnEnable()
    {
        handle.selectEntered.AddListener(OnGrab);
        handle.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        handle.selectEntered.RemoveListener(OnGrab);
        handle.selectExited.RemoveListener(OnRelease);
    }

    private void FixedUpdate()
    {
        if (!isGrabbed || grabbingHand == null) return;

        // Direction from mast pivot to hand
        Vector3 handDir = grabbingHand.position - mastRigidbody.position;

        // Cross with mast right to get torque direction
        Vector3 torque = Vector3.Cross(mastRigidbody.transform.right, handDir) * torqueMultiplier;

        mastRigidbody.AddTorque(torque, ForceMode.Force);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        grabbingHand = args.interactorObject.transform;
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        grabbingHand = null;
    }
}

