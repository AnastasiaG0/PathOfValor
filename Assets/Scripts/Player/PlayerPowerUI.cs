using UnityEngine;

public class PlayerPowerUI : MonoBehaviour
{
    private TextMesh powerText;
    private PlayerCombat playerCombat;

    void Start()
    {
        powerText = GetComponent<TextMesh>();
        playerCombat = GetComponentInParent<PlayerCombat>();

        if (powerText == null)
            Debug.LogError("TextMesh не найден!");
    }

    void Update()
    {
        if (playerCombat != null && powerText != null)
        {
            powerText.text = playerCombat.currentPower.ToString();
        }
    }
}