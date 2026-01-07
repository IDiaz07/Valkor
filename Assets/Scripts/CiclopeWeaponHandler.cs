using UnityEngine;
using UnityEngine.AI;

public class CiclopeWeaponHandler : MonoBehaviour
{

    public GameObject sword;

    // English bone names
    public string handBone = "mixamorig:RightHand";
    public string backBone = "mixamorig:Spine2";

    private Transform handTransform;
    private Transform backTransform;

    void Start()
    {

        // Find bones in hierarchy
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {

            if (t.name == handBone)
                handTransform = t;

            if (t.name == backBone)
                backTransform = t;
        }

        // Start with sword on back
        if (sword != null && backTransform != null)
        {

            sword.transform.SetParent(backTransform);
            sword.transform.localPosition = Vector3.zero;
            sword.transform.localRotation = Quaternion.identity;
        }
    }

    // 🎯 THIS EVENT MUST BE IN TAKE SWORD 1
    public void OnTakeSword1Started()
    {

        if (sword == null || handTransform == null)
            return;

        // Move sword from back to hand
        sword.transform.SetParent(handTransform);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;

        Debug.Log("Sword moved to hand in Take Sword 1");
    }

    // optional reset
    public void PutSwordOnBack()
    {

        if (sword == null || backTransform == null)
            return;

        sword.transform.SetParent(backTransform);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }
}
