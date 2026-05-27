using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Тексты")]
    public Text powerText;
    public Text killsText;
    public Text hintText;

    [Header("Полосы прогресса")]
    public Slider powerProgressBar;
    public Slider healthBar;

    [Header("Анимации")]
    public Animator uiAnimator;

    private PlayerCombat playerCombat;
    private int maxPower = 50;
    private float hintTimer = 0f;

    void Start()
    {
        playerCombat = FindObjectOfType<PlayerCombat>();

        // Показываем подсказку в начале
        if (hintText != null)
        {
            hintText.text = "Управление: WASD или Стрелки\nАтакуй врагов слабее себя!";
            hintTimer = 5f;
        }
    }

    void Update()
    {
        // Обновление UI
        if (playerCombat != null)
        {
            if (powerText != null)
                powerText.text = $"СИЛА: {playerCombat.currentPower}";

            if (powerProgressBar != null)
                powerProgressBar.value = (float)playerCombat.currentPower / maxPower;
        }

        // Обновление счётчика убийств
        if (GameManager.Instance != null && killsText != null)
        {
            killsText.text = $"УБИТО: {GetEnemiesKilled()}";
        }

        // Скрытие подсказки через время
        if (hintTimer > 0)
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0 && hintText != null)
            {
                hintText.gameObject.SetActive(false);
            }
        }
    }

    int GetEnemiesKilled()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        int killed = 0;
        foreach (Enemy e in enemies)
        {
            if (!e.IsAlive)
                killed++;
        }
        return killed;
    }
}