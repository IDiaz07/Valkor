using UnityEngine;
using UnityEngine.UI;

public class WallHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private Transform target;

    public void Setup(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void UpdateHealth(float current, float max)
    {
        slider.value = current / max;
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + Vector3.up * 2f;
            transform.forward = Camera.main.transform.forward; // mira al jugador
        }
    }
}