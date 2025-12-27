using Unity.Collections;
using UnityEngine;

public class Craftable : MonoBehaviour
{
    public GameObject craftable;

    [System.Serializable]
    public struct Requirement
    {
        public GameObject requirement;
        [Range(1,999)] public int amount;
        public int itemID;

    }
    public Requirement[] requirements;
}
