using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public LayerMask wallLayer;

    [Header("Настройки боя")]
    public float attackRange = 2f;           // Дистанция атаки
    public LayerMask enemyLayer;             // Слой врагов

    [Header("Настройки комбо")]
    public float comboWindow = 0.8f;
    public float attackCooldown = 0.5f;

    [Header("Компоненты")]
    private Rigidbody rb;
    private PlayerCombat combat;
    private Animator animator;

    [Header("Состояние")]
    private Vector3 moveDirection;
    private bool canMove = true;
    private bool isFighting = false;
    private float currentSpeed = 0f;
    private bool isGrounded = true;

    // Параметры для комбо-атак
    private int comboIndex = 0;
    private float lastAttackTime = 0f;
    private bool canCombo = false;
    private bool isAttacking = false;

    // Ближайший враг для атаки
    private Enemy targetEnemy = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        // Настройка Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;

        combat = GetComponent<PlayerCombat>();
        animator = GetComponent<Animator>();

        // Настройка слоёв
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer != -1)
        {
            wallLayer |= (1 << obstacleLayer);
        }
        wallLayer |= LayerMask.GetMask("Default");

        // Если слой врагов не задан, ищем по тегу
        if (enemyLayer == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }

        if (animator == null)
            Debug.LogError("Animator не найден!");
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            moveDirection = Vector3.zero;
            UpdateAnimations();
            return;
        }

        // ПОИСК БЛИЖАЙШЕГО ВРАГА для атаки
        FindNearestEnemy();

        // Ввод движения (только если не в бою и не атакует)
        float horizontal = 0;
        float vertical = 0;

        if (!isFighting && !isAttacking && canMove && !IsDead())
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Вычисление скорости для анимаций и движения
        if (moveDirection.magnitude > 0.1f && !isFighting && !isAttacking && !IsDead())
        {
            // Shift для бега
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSpeed = runSpeed;    // 6 для физики
            }
            else
            {
                currentSpeed = walkSpeed;   // 3 для физики
            }
        }
        else
        {
            currentSpeed = 0f;
        }

        UpdateAnimations();

        // Обработка атаки
        if (Input.GetButtonDown("Fire1") && !isFighting && !isAttacking && !IsDead())
        {
            // Проверяем, есть ли враг в радиусе атаки
            if (IsEnemyInRange())
            {
                Attack();
            }
            else
            {
                Debug.Log("Слишком далеко от врага для атаки!");
            }
        }

        UpdatePlayerHeight();
    }

    // Поиск ближайшего врага
    void FindNearestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        float closestDistance = attackRange;
        targetEnemy = null;

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetEnemy = enemy;
                }
            }
        }
    }

    // Проверка, есть ли враг в радиусе атаки
    bool IsEnemyInRange()
    {
        return targetEnemy != null;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Определяем целевую скорость анимации
        float targetAnimSpeed = 0f;
        
        if (moveDirection.magnitude > 0.1f && !isFighting && !isAttacking && !IsDead())
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                targetAnimSpeed = 1f;   // Бег (должно быть > 0.5 в Animator)
            }
            else
            {
                targetAnimSpeed = 0.3f; // Ходьба (должно быть > 0.1 в Animator)
            }
        }
        
        // Плавное изменение скорости анимации
        float currentAnimSpeed = animator.GetFloat("Speed");
        float newAnimSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, Time.deltaTime * 10f);
        
        animator.SetFloat("Speed", newAnimSpeed);
        //animator.SetFloat("RawSpeed", currentSpeed);
        animator.SetBool("IsMoving", moveDirection.magnitude > 0.1f && !isFighting && !isAttacking);

        if (moveDirection.magnitude > 0.1f)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveZ", moveDirection.z);
        }

        //animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsFighting", isFighting);
    }

    void Attack()
    {
        if (animator == null) return;

        // Если врага нет в радиусе — не атакуем
        if (!IsEnemyInRange())
        {
            Debug.Log("Нет врага в радиусе атаки!");
            return;
        }

        float timeSinceLastAttack = Time.time - lastAttackTime;

        if (canCombo && timeSinceLastAttack < comboWindow)
        {
            comboIndex = (comboIndex + 1) % 2;
            animator.SetInteger("ComboIndex", comboIndex);
            animator.SetTrigger("Attack");
            canCombo = false;
        }
        else
        {
            comboIndex = 0;
            animator.SetInteger("ComboIndex", comboIndex);
            animator.SetTrigger("Attack");
            canCombo = true;
        }

        lastAttackTime = Time.time;
        StartCoroutine(AttackSequence());

        // Наносим урон врагу (если нужно)
        if (targetEnemy != null && combat != null)
        {
            StartCoroutine(FightSequence(targetEnemy));
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;

        yield return new WaitForSeconds(comboWindow - attackCooldown);
        if (!isAttacking)
        {
            canCombo = false;
            comboIndex = 0;
            if (animator != null)
                animator.SetInteger("ComboIndex", 0);
        }
    }

    void UpdatePlayerHeight()
    {
        Vector3 rayStart = transform.position + Vector3.up * 3f;
        float rayLength = 8f;
        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength, groundLayerMask))
        {
            float targetHeight = hit.point.y + 0.5f;
            float maxHeightChange = 8f * Time.deltaTime;
            float newY = Mathf.MoveTowards(transform.position.y, targetHeight, maxHeightChange);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // ДВИЖЕНИЕ — только если не в бою и не атакует
        if (canMove && !isFighting && !isAttacking && !IsDead() && moveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            
            // Сохраняем вертикальную скорость (гравитация)
            targetVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = targetVelocity;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Торможение: сохраняем только вертикальную скорость
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    bool WillCollideWithWall(Vector3 targetPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.4f, wallLayer);
        return colliders.Length > 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null && enemy.IsAlive && combat != null && !isFighting)
        {
            // Атакуем врага при касании
            StartCoroutine(FightSequence(enemy));
        }

        if (collision.gameObject.CompareTag("Castle"))
        {
            GameManager.Instance?.GameOver(true);
        }
    }

    IEnumerator FightSequence(Enemy enemy)
    {
        if (isFighting) yield break;

        isFighting = true;
        Debug.Log("Битва началась! Движение заблокировано.");

        if (animator != null)
        {
            animator.SetBool("IsFighting", true);
        }

        combat.FightEnemy(enemy);

        yield return new WaitForSeconds(0.5f);

        isFighting = false;
        if (animator != null)
        {
            animator.SetBool("IsFighting", false);
        }
        Debug.Log("Битва закончилась! Движение разблокировано.");
    }

    public void DisableMovement()
    {
        canMove = false;
        moveDirection = Vector3.zero;
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsMoving", false);
        }
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void TakeDamage()
    {
        if (animator != null && !IsDead())
        {
            animator.SetTrigger("GetHit");
        }
    }

    public void Die()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        canMove = false;
        isFighting = false;
        isAttacking = false;
    }

    public void Revive()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }
        canMove = true;
    }

    private bool IsDead()
    {
        if (animator != null)
        {
            return animator.GetBool("IsDead");
        }
        return false;
    }

    // Визуализация радиуса атаки в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}