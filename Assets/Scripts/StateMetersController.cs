using UnityEngine;
using UnityEngine.UI;

public class StateMetersController : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    public CharacterLife playerStats;
    [SerializeField]
    private GameObject healthMeter;
    [SerializeField]
    private GameObject staminaMeter;
    private Image healthBar;
    private Image staminaBar;

    private void Awake()
    {
        healthBar = healthMeter.transform.GetChild(0).GetComponent<Image>();
        staminaBar = staminaMeter.transform.GetChild(0).GetComponent<Image>();
        playerStats = player.GetComponent<CharacterLife>();
    }
    private void Update()
    {
        healthBar.transform.localScale = new Vector3(playerStats.ActualHealth / playerStats.MaxHealth,1,1);
        staminaBar.transform.localScale = new Vector3(playerStats.ActualStamina / playerStats.MaxStamina, 1, 1);
    }
}
