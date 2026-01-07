using UnityEngine;

public class CiclopeWeaponHandler : MonoBehaviour
{

    public GameObject sword;

    // Bones names
    public string handBone = "mixamorig:RightHand";

    private Transform handTransform;
    private Transform backTransform;

    void Start()
    {

        // Find bones in hierarchy
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {

            if (t.name == handBone)
                handTransform = t;

            if (t.name.Contains("Spine") || t.name.Contains("Back"))
                backTransform = t;
        }

        if (sword != null && backTransform != null)
        {

            // Keep in back at start
            sword.transform.SetParent(backTransform);

            sword.transform.localPosition = Vector3.zero;
            sword.transform.localRotation = Quaternion.identity;
        }
    }

    // 🎯 Called from Animation Event – frame 3
    public void OnGrabSword()
    {

        if (sword == null || handTransform == null)
            return;

        // Move to hand in English
        Transform tr = sword.transform;

        tr.SetParent(handTransform);

        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;

        Debug.Log("Sword attached to hand");
    }

    public void PutSwordOnBack()
    {

        if (sword == null || backTransform == null)
            return;

        Transform tr = sword.transform;

        tr.SetParent(backTransform);

        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
    }
}
