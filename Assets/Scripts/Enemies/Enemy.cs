using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Характеристики врага")]
    public int power = 3;
    public bool IsAlive { get; private set; } = true;

    [Header("Настройки атаки")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;

    [Header("Настройки обнаружения")]
    public float detectionRange = 8f;
    public float rotationSpeed = 5f;

    [Header("Визуализация")]
    public bool showGizmos = true;

    [Header("Компоненты")]
    public Animator animator;

    public System.Action OnAttackFinished;  // ← СОБЫТИЕ ОКОНЧАНИЯ АТАКИ
    public System.Action OnDeathStarted;    // ← СОБЫТИЕ НАЧАЛА СМЕРТИ

    private Transform player;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isPlayerDetected = false;
    private bool isAttackComplete = false;   // Флаг завершения атаки

    void Start()
    {
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (!IsAlive) return;
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            isPlayerDetected = true;
            RotateTowardsPlayer();
        }
        else
        {
            isPlayerDetected = false;
        }

        if (distanceToPlayer <= attackRange && !isAttacking && !isDead)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
            }
        }

        UpdateAnimator();
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
        animator.SetBool("IsPlayerDetected", isPlayerDetected);
    }

    void Attack()
    {
        if (!IsAlive) return;
        if (isDead) return;
        if (isAttacking) return;

        lastAttackTime = Time.time;
        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        isAttackComplete = false;

        RotateTowardsPlayer();

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // ПОЛУЧАЕМ ДЛИТЕЛЬНОСТЬ АНИМАЦИИ АТАКИ
        float attackAnimationLength = GetAttackAnimationLength();

        Debug.Log($"Атака врага началась, длительность: {attackAnimationLength} сек");

        // Ждём окончания анимации атаки
        yield return new WaitForSeconds(attackAnimationLength);

        isAttacking = false;
        isAttackComplete = true;

        // ВЫЗЫВАЕМ СОБЫТИЕ ОКОНЧАНИЯ АТАКИ
        OnAttackFinished?.Invoke();

        Debug.Log("Атака врага завершена");
    }

    float GetAttackAnimationLength()
    {
        if (animator == null) return 0.5f;

        // Пытаемся получить длительность анимации "Attack"
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains("Attack"))
            {
                return clip.length;
            }
        }
        return 0.8f; // Значение по умолчанию
    }

    public void StartDeath()
    {
        if (!IsAlive) return;
        if (isDead) return;

        IsAlive = false;
        isDead = true;
        isAttacking = false;

        // ВЫЗЫВАЕМ СОБЫТИЕ НАЧАЛА СМЕРТИ
        OnDeathStarted?.Invoke();

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log("Враг начал анимацию смерти");
    }

    public void Die()
    {
        StartDeath(); // Вызываем начало смерти
    }

    public void PlayAttackAnimation()
    {
        if (animator != null && IsAlive && !isDead)
        {
            animator.SetTrigger("Attack");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}