using UnityEngine;
using System.Collections;

public enum AttackType
{
    Sword,      // Меч
    Arrow,      // Стрела/плевок
    Fire,       // Огонь
    Throw,      // Метание
    HeavyStrike // Тяжёлый удар
}

public class Enemy : MonoBehaviour
{
    [Header("Характеристики врага")]
    public int power = 3;
    public AttackType attackType = AttackType.Sword;

    [Header("Визуальные эффекты")]
    public GameObject powerLabelPrefab;
    public GameObject attackEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject flashOnDeathPrefab;
    public GameObject powerUpEffectPrefab;

    [Header("Звуки")]
    public AudioClip[] attackSounds;
    public AudioClip deathSound;

    [Header("Программная загрузка эффектов")]
    public bool loadEffectsProgrammatically = true;

    public bool IsAlive { get; private set; } = true;

    private TextMesh powerDisplay;
    public Animator animator;
    private AudioSource audioSource;
    private Renderer enemyRenderer;

    void Start()
    {
        // ========== ПРОГРАММНАЯ ЗАГРУЗКА ЭФФЕКТОВ ==========
        if (loadEffectsProgrammatically)
        {
            LoadEffectsProgrammatically();
        }
        // ========== КОНЕЦ ЗАГРУЗКИ ЭФФЕКТОВ ==========

        // Создание метки силы (ВСЕГДА ВИДНА)
        if (powerLabelPrefab != null)
        {
            GameObject label = Instantiate(powerLabelPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            label.transform.SetParent(transform);
            powerDisplay = label.GetComponent<TextMesh>();
            if (powerDisplay != null)
                powerDisplay.text = power.ToString();
        }
        else
        {
            // Создаём текстовую метку программно
            GameObject label = new GameObject("PowerLabel");
            label.transform.SetParent(transform);
            label.transform.localPosition = new Vector3(0, 1.2f, 0);
            label.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            powerDisplay = label.AddComponent<TextMesh>();
            powerDisplay.fontSize = 30;
            powerDisplay.color = Color.yellow;
            powerDisplay.anchor = TextAnchor.MiddleCenter;
            powerDisplay.alignment = TextAlignment.Center;
            powerDisplay.text = power.ToString();
        }

        // Получаем компоненты
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        enemyRenderer = GetComponent<Renderer>();

        // Настройка цвета в зависимости от силы
        if (enemyRenderer != null)
        {
            // Оттенки красного: слабые - светлые, сильные - тёмные
            float intensity = 0.5f + (float)power / 20f;
            Color enemyColor = new Color(0.8f, 0.2f - (float)power / 50f, 0.1f);
            enemyRenderer.material.color = enemyColor;
        }

        // Настройка анимации
        if (animator != null)
        {
            animator.SetInteger("AttackType", (int)attackType);
        }

        Debug.Log($"Враг создан: сила = {power}, тип атаки = {attackType}");
    }

    public void Die()
    {
        if (!IsAlive) return;

        IsAlive = false;

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRandomSound("EnemyDeath");
        }

        if (flashOnDeathPrefab != null)
        {
            GameObject flash = Instantiate(flashOnDeathPrefab, transform.position, Quaternion.identity);
            Destroy(flash, 0.5f);
        }

        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        if (powerUpEffectPrefab != null)
        {
            GameObject powerEffect = Instantiate(powerUpEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(powerEffect, 1f);
        }

        if (powerDisplay != null)
            Destroy(powerDisplay.gameObject);

        Destroy(gameObject, 0.3f);
    }

    public void PlayAttackAnimation()
    {
        if (!IsAlive) return;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (attackEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + transform.forward * 0.8f;
            GameObject effect = Instantiate(attackEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        if (audioSource != null && attackSounds.Length > 0)
        {
            int soundIndex = Mathf.Min((int)attackType, attackSounds.Length - 1);
            audioSource.PlayOneShot(attackSounds[soundIndex]);
        }
        else if (AudioManager.Instance != null)
        {
            switch (attackType)
            {
                case AttackType.Sword:
                    AudioManager.Instance.PlayRandomSound("Sword");
                    break;
                case AttackType.Arrow:
                    AudioManager.Instance.PlaySound("Arrow");
                    break;
                case AttackType.Fire:
                    AudioManager.Instance.PlaySound("Fire");
                    break;
                case AttackType.Throw:
                    AudioManager.Instance.PlaySound("Throw");
                    break;
                case AttackType.HeavyStrike:
                    AudioManager.Instance.PlaySound("Heavy");
                    break;
            }
        }
    }

    public void ShowAttackIndicator()
    {
        if (enemyRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
    }

    private IEnumerator FlashRed()
    {
        Color originalColor = enemyRenderer.material.color;
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.material.color = originalColor;
    }

    public void UpdatePowerDisplay()
    {
        if (powerDisplay != null)
        {
            powerDisplay.text = power.ToString();
        }
    }

    /// <summary>
    /// Программно загружает эффекты из Resources или по тегу
    /// </summary>
    void LoadEffectsProgrammatically()
    {
        // Загрузка DeathEffect
        if (deathEffectPrefab == null)
        {
            deathEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/DeathEffect");
            if (deathEffectPrefab == null)
                deathEffectPrefab = GameObject.FindGameObjectWithTag("DeathEffect");
            if (deathEffectPrefab != null)
                Debug.Log($"DeathEffect загружен программно для {gameObject.name}");
        }

        // Загрузка FlashOnDeath
        if (flashOnDeathPrefab == null)
        {
            flashOnDeathPrefab = Resources.Load<GameObject>("Prefabs/Effects/FlashOnDeath");
            if (flashOnDeathPrefab == null)
                flashOnDeathPrefab = GameObject.FindGameObjectWithTag("FlashOnDeath");
            if (flashOnDeathPrefab != null)
                Debug.Log($"FlashOnDeath загружен программно для {gameObject.name}");
        }

        // Загрузка PowerUpEffect
        if (powerUpEffectPrefab == null)
        {
            powerUpEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/PowerUpEffect");
            if (powerUpEffectPrefab == null)
                powerUpEffectPrefab = GameObject.FindGameObjectWithTag("PowerUpEffect");
            if (powerUpEffectPrefab != null)
                Debug.Log($"PowerUpEffect загружен программно для {gameObject.name}");
        }

        // Загрузка AttackEffect (в зависимости от типа атаки)
        if (attackEffectPrefab == null)
        {
            string effectName = GetAttackEffectName(attackType);
            attackEffectPrefab = Resources.Load<GameObject>($"Prefabs/Effects/{effectName}");
            if (attackEffectPrefab == null)
                attackEffectPrefab = GameObject.FindGameObjectWithTag(effectName);
            if (attackEffectPrefab != null)
                Debug.Log($"{effectName} загружен программно для {gameObject.name}");
        }
    }

    /// <summary>
    /// Возвращает имя эффекта в зависимости от типа атаки
    /// </summary>
    string GetAttackEffectName(AttackType type)
    {
        switch (type)
        {
            case AttackType.Sword: return "AttackEffect_Sword";
            case AttackType.Arrow: return "AttackEffect_Arrow";
            case AttackType.Fire: return "AttackEffect_Fire";
            case AttackType.Throw: return "AttackEffect_Throw";
            case AttackType.HeavyStrike: return "AttackEffect_Heavy";
            default: return "AttackEffect";
        }
    }
}
