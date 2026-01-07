using UnityEngine;

public class CiclopeWeaponHandler : MonoBehaviour
{

    public GameObject Espada;
    public Transform CiclopeHandPoint;
    public Transform CiclopeBackPoint;

    public void GrabSword()
    {

        if (Espada == null ||
            CiclopeHandPoint == null)
            return;

        Espada.transform.SetParent(
            CiclopeHandPoint
        );

        Espada.transform.localPosition =
            Vector3.zero;

        Espada.transform.localRotation =
            Quaternion.identity;
    }

    public void PutOnBack()
    {

        if (Espada == null ||
            CiclopeBackPoint == null)
            return;

        Espada.transform.SetParent(
            CiclopeBackPoint
        );

        Espada.transform.localPosition =
            Vector3.zero;

        Espada.transform.localRotation =
            Quaternion.identity;
    }
}
