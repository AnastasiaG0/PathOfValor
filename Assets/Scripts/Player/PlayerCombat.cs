using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Характеристики")]
    public int currentPower = 5;
    public int maxPower = 1000;

    [Header("Эффекты")]
    public GameObject victoryEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject powerUpEffectPrefab;
    public AudioClip victorySound;
    public AudioClip deathSound;

    [Header("UI")]
    public TextMeshProUGUI powerText;

    private AudioSource audioSource;
    private Renderer[] renderers;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        renderers = GetComponentsInChildren<Renderer>();

        // Поиск PowerText
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
        // Воспроизводим анимацию атаки врага
        enemy.PlayAttackAnimation();

        // Сравниваем силу
        if (currentPower > enemy.power)
        {
            // ПОБЕДА
            StartCoroutine(VictorySequence(enemy));
        }
        else
        {
            // ПОРАЖЕНИЕ
            Die();
        }
    }

    IEnumerator VictorySequence(Enemy enemy)
    {
        // Заморозка на секунду для эффекта
        yield return new WaitForSeconds(0.3f);

        // Увеличиваем силу
        //int oldPower = currentPower;
        currentPower += enemy.power;
        if (currentPower > maxPower)
            currentPower = maxPower;

        UpdatePowerUI();

        // Эффект вспышки
        if (victoryEffectPrefab != null)
        {
            GameObject flash = Instantiate(victoryEffectPrefab, transform.position, Quaternion.identity);
            Destroy(flash, 0.5f);
        }

        // Воспроизводим звук победы
        if (audioSource != null && victorySound != null)
            audioSource.PlayOneShot(victorySound);

        // Эффект получения силы
        if (powerUpEffectPrefab != null)
        {
            GameObject powerUp = Instantiate(powerUpEffectPrefab, transform.position, Quaternion.identity);
            Destroy(powerUp, 1.5f);
        }

        // Визуальный эффект
        if (victoryEffectPrefab != null)
        {
            GameObject effect = Instantiate(victoryEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Уведомляем GameManager о победе над врагом
        if (GameManager.Instance != null)
            GameManager.Instance.EnemyKilled(enemy.power);

        // Уничтожаем врага
        enemy.Die();
    }

    IEnumerator FlashEffect(Color color)
    {
        // Сохраняем оригинальные цвета
        Material[] originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            renderers[i].material.SetColor("_EmissionColor", color * 2f);
        }

        yield return new WaitForSeconds(0.2f);

        // Восстанавливаем цвета
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_EmissionColor", Color.black);
        }
    }

    void UpdatePowerUI()
    {
        if (powerText != null)
            powerText.text = "СИЛА: " + currentPower;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerPower(currentPower);
    }

    void Die()
    {
        // Эффект смерти
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Звук смерти
        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        // Уведомляем GameManager о поражении
        GameManager.Instance.GameOver(false);

        // Отключаем игрока
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            GameManager.Instance.GameOver(true);
        }
    }
}