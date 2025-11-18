using UnityEngine;

public class SpawneableEffectController : MonoBehaviour
{
    [SerializeField]
    private float timeToLive;
    void Awake()
    {
        Destroy(gameObject, timeToLive);
    }

}
