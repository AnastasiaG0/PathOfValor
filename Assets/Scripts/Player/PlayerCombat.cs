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

        // ЛОГИКА БОЯ - СРАВНЕНИЕ СИЛЫ
        if (currentPower > enemy.power)
        {
            // ПОБЕДА ИГРОКА
            Debug.Log($"Игрок победил! {currentPower} > {enemy.power}");
            StartCoroutine(VictorySequence(enemy));
        }
        else
        {
            // ПОРАЖЕНИЕ ИГРОКА (ВРАГ ПОБЕДИЛ)
            Debug.Log($"Игрок проиграл! {currentPower} <= {enemy.power}");

            // НОВЫЙ ФЛАГ: сообщаем врагу о победе
            enemy.SetVictorious();

            Die();
        }
    }

    IEnumerator VictorySequence(Enemy enemy)
    {
        currentPower += enemy.power;
        if (currentPower > maxPower)
            currentPower = maxPower;

        UpdatePowerUI();

        if (GameManager.Instance != null)
            GameManager.Instance.EnemyKilled(enemy.power);

        enemy.Die();

        float deathAnimLength = GetAnimationLength(enemy.animator, "Die");
        yield return new WaitForSeconds(deathAnimLength);

        Debug.Log("Анимация смерти врага завершена, игрок может двигаться");

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetCanMoveAfterVictory(true);
        }
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

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Die();
        }

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

    private float GetAnimationLength(Animator anim, string animationName)
    {
        if (anim == null) return 0.5f;

        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains(animationName))
            {
                return clip.length;
            }
        }
        return 0.5f;
    }
}