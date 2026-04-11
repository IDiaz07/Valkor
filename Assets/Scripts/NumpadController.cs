using TMPro;
using UnityEngine;

public class NumpadController : MonoBehaviour
{
    public TMP_InputField ipInputField;

    public void AddCharacter(string character)
    {
        ipInputField.text += character;
    }

    public void Backspace()
    {
        if (ipInputField.text.Length > 0)
        {
            ipInputField.text = ipInputField.text.Substring(0, ipInputField.text.Length - 1);
        }
    }
}
