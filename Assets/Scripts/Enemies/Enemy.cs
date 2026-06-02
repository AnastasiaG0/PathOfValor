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

    [Header("Дополнительное обнаружение для аниматора")]
    public bool useExtraDetection = false;           // Включить дополнительное обнаружение
    public float extraDetectionRange = 10f;          // Дальность дополнительного обнаружения
    public bool detectOnlyInFront = false;           // Обнаруживать только спереди
    public float frontAngle = 90f;                   // Угол обзора (если detectOnlyInFront = true)

    [Header("Визуализация")]
    public bool showGizmos = true;

    [Header("Компоненты")]
    public Animator animator;

    [Header("Куб силы над головой")]
    public CubeRotator headCube;

    public System.Action OnAttackFinished;
    public System.Action OnDeathStarted;
    public System.Action OnHitMoment;

    private Transform player;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isPlayerDetected = false;
    private bool isAttackComplete = false;

    private bool isPlayerInExtraRange = false;
    private bool isVictorious = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // Начальная проверка для куба (если игрок уже в зоне)
            if (headCube != null && useExtraDetection)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                bool playerInExtraRange = distanceToPlayer <= extraDetectionRange;

                if (playerInExtraRange)
                {
                    headCube.FadeIn();
                }
                else
                {
                    headCube.FadeOut();
                }
            }
        }
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

        if (useExtraDetection)
        {
            bool playerInRange = distanceToPlayer <= extraDetectionRange;
            bool playerInFront = true;

            if (detectOnlyInFront && playerInRange)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                playerInFront = angleToPlayer <= frontAngle / 2f;
            }

            isPlayerInExtraRange = playerInRange && (!detectOnlyInFront || playerInFront);
        }
        else
        {
            isPlayerInExtraRange = false;
        }

        if (headCube != null)
        {
            if (isPlayerInExtraRange)
            {
                headCube.FadeIn();
            }
            else
            {
                headCube.FadeOut();
            }
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
        animator.SetBool("IsPlayerInExtraRange", isPlayerInExtraRange);
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
        if (!IsAlive) yield break;
        if (isDead) yield break;
        if (isAttacking) yield break;

        isAttacking = true;

        RotateTowardsPlayer();

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Debug.Log($"Атака врага началась, ждём Animation Event...");

        while (isAttacking && IsAlive && !isDead)
        {
            yield return null;
        }

        if (!IsAlive || isDead)
        {
            Debug.Log("Враг умер во время атаки, событие не вызывается");
            yield break;
        }

        Debug.Log("Корутина атаки врага завершена");
    }

    float GetAttackAnimationLength()
    {
        if (animator == null) return 0.5f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains("Attack"))
            {
                return clip.length;
            }
        }
        return 0.8f;
    }

    public void SetVictorious()
    {
        if (!IsAlive) return;
        if (isDead) return;

        Debug.Log($"{name}: Враг победил! Активирована анимация победы");

        if (animator != null)
        {
            animator.SetBool("IsVictorious", true);
        }
    }

    public void OnAttackAnimationFinished()
    {
        if (!IsAlive) return;
        if (isDead) return;
        if (!isAttacking) return;

        Debug.Log($"{name}: Анимация атаки закончилась (Animation Event)");

        isAttacking = false;

        OnAttackFinished?.Invoke();
    }

    public void OnHitMomentAnimationEvent()
    {
        if (!IsAlive) return;
        if (isDead) return;

        Debug.Log($"{name}: Момент удара! Игрок синхронизирует атаку");
        OnHitMoment?.Invoke();
    }

    public void StartDeath()
    {
        if (!IsAlive) return;
        if (isDead) return;

        IsAlive = false;
        isDead = true;
        isAttacking = false;

        OnDeathStarted?.Invoke();

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (headCube != null)
        {
            headCube.Pop();

            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
        }
        else
        {
            // Если куба нет, просто умираем
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
        }
    }

    public void Die()
    {
        StartDeath();
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

        // СУЩЕСТВУЮЩИЕ GIZMOS (НЕ ТРОГАЕМ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // НОВЫЕ GIZMOS ДЛЯ ДОПОЛНИТЕЛЬНОГО ОБНАРУЖЕНИЯ
        if (useExtraDetection)
        {
            if (detectOnlyInFront)
            {
                Gizmos.color = Color.cyan;
                Vector3 forward = transform.forward;
                Vector3 right = Quaternion.Euler(0, frontAngle / 2f, 0) * forward;
                Vector3 left = Quaternion.Euler(0, -frontAngle / 2f, 0) * forward;

                Gizmos.DrawRay(transform.position, right * extraDetectionRange);
                Gizmos.DrawRay(transform.position, left * extraDetectionRange);

                // Рисуем дугу
                float radius = extraDetectionRange;
                float angleStep = frontAngle / 20f;
                Vector3 prevPoint = transform.position + Quaternion.Euler(0, -frontAngle / 2f, 0) * forward * radius;
                for (float a = -frontAngle / 2f; a <= frontAngle / 2f; a += angleStep)
                {
                    Vector3 point = transform.position + Quaternion.Euler(0, a, 0) * forward * radius;
                    Gizmos.DrawLine(prevPoint, point);
                    prevPoint = point;
                }
            }
            else
            {
                Gizmos.color = new Color(0, 1, 1, 0.3f); // Полупрозрачный голубой
                Gizmos.DrawWireSphere(transform.position, extraDetectionRange);
            }
        }
    }
}