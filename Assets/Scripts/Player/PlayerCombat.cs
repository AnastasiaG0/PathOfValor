using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Характеристики")]
    public int currentPower = 5;
    public int maxPower = 1000;

    [Header("UI")]
    public TextMeshProUGUI powerText;

    private AudioSource audioSource;
    private bool isDead = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (powerText == null)
        {
            GameObject textObj = GameObject.Find("PowerText");
            if (textObj != null)
                powerText = textObj.GetComponent<TextMeshProUGUI>();
        }

        UpdatePowerUI();
    }

    public void FightEnemy(Enemy enemy)
    {
        if (isDead) return;

        // Воспроизводим анимацию атаки врага
        enemy.PlayAttackAnimation();

        // ЛОГИКА БОЯ
        if (currentPower > enemy.power)
        {
            // ПОБЕДА - враг умирает
            StartCoroutine(VictorySequence(enemy));
        }
        else
        {
            // ПОРАЖЕНИЕ - игрок умирает
            Die();
        }
    }

    IEnumerator VictorySequence(Enemy enemy)
    {
        yield return new WaitForSeconds(0.3f);

        // Увеличиваем силу
        currentPower += enemy.power;
        if (currentPower > maxPower)
            currentPower = maxPower;

        UpdatePowerUI();

        // Уведомляем GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.EnemyKilled(enemy.power);

        // ВРАГ УМИРАЕТ
        enemy.Die();
    }

    void UpdatePowerUI()
    {
        if (powerText != null)
            powerText.text = "СИЛА: " + currentPower;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerPower(currentPower);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Анимация смерти игрока
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Die();
        }

        // Уведомляем GameManager о поражении
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            GameManager.Instance.GameOver(true);
        }
    }
}