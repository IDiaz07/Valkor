using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{


    public GameObject IteminShot;
    public Image slotImage;
    Color originalColor;
  
    void Start()
    {
        slotImage = GetComponentInChildren<Image>();
         originalColor = slotImage.color;
    }

 
}
