using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public LayerMask wallLayer;

    [Header("Настройки атаки")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    public LayerMask enemyLayer;

    [Header("Компоненты")]
    private Rigidbody rb;
    private PlayerCombat combat;
    private Animator animator;
    private Camera playerCamera;

    [Header("Состояние")]
    private Vector3 moveDirection;
    private bool canMove = true;
    private bool isFighting = false;
    private float currentSpeed = 0f;
    private bool isGrounded = true;

    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private Coroutine currentAttackCoroutine = null;

    private Enemy targetEnemy = null;
    private bool enemyIsAttacking = false;
    private bool waitingForEnemyAttack = false;
    private bool canMoveAfterVictory = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;

        combat = GetComponent<PlayerCombat>();
        animator = GetComponent<Animator>();

        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Camera cam = FindObjectOfType<Camera>();
            if (cam != null)
                playerCamera = cam;
        }

        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer != -1)
        {
            wallLayer |= (1 << obstacleLayer);
        }
        wallLayer |= LayerMask.GetMask("Default");

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

        FindNearestEnemy();

        if (targetEnemy != null && targetEnemy.IsAlive)
        {
            targetEnemy.OnAttackFinished -= OnEnemyAttackFinished;
            targetEnemy.OnAttackFinished += OnEnemyAttackFinished;

            targetEnemy.OnDeathStarted -= OnEnemyDeathStarted;
            targetEnemy.OnDeathStarted += OnEnemyDeathStarted;

            // НОВЫЕ ПОДПИСКИ
            targetEnemy.OnHitMoment -= OnEnemyHitMoment;
            targetEnemy.OnHitMoment += OnEnemyHitMoment;
        }

        if (targetEnemy != null && !targetEnemy.IsAlive)
        {
            if (targetEnemy != null)
            {
                targetEnemy.OnAttackFinished -= OnEnemyAttackFinished;
                targetEnemy.OnDeathStarted -= OnEnemyDeathStarted;
                targetEnemy.OnHitMoment -= OnEnemyHitMoment;  // НОВОЕ
            }
            targetEnemy = null;
            waitingForEnemyAttack = false;
            enemyIsAttacking = false;
        }

        float horizontal = 0;
        float vertical = 0;
        bool canAct = !isFighting && !isAttacking && canMove && !IsDead() && canMoveAfterVictory;

        if (canAct)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        if (playerCamera != null && (horizontal != 0 || vertical != 0) && canAct)
        {
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        if (moveDirection.magnitude > 0.1f && canAct)
        {
            currentSpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? runSpeed : walkSpeed;
        }
        else
        {
            currentSpeed = 0f;
        }

        UpdateAnimations();

        if (targetEnemy != null && targetEnemy.IsAlive && !isAttacking && !isFighting && !IsDead() && !waitingForEnemyAttack)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
            }
        }

        UpdatePlayerHeight();
    }

    void OnEnemyAttackFinished()
    {
        Debug.Log("Получен сигнал от врага: анимация атаки завершена!");
        waitingForEnemyAttack = false;
        enemyIsAttacking = false;
    }

    void OnEnemyDeathStarted()
    {
        Debug.Log("Враг начал умирать - игрок завершает атаку");

        if (isAttacking)
        {
            FinishAttackEarly();
        }
    }

    // НОВЫЙ МЕТОД: вызывается в момент удара врага
    void OnEnemyHitMoment()
    {
        Debug.Log("Враг нанёс удар! Игрок проигрывает анимацию удара");

        if (animator != null)
        {
            animator.SetTrigger("HitMoment");
        }
    }

    void FinishAttackEarly()
    {
        if (!isAttacking) return;

        Debug.Log("Досрочное завершение атаки игрока");

        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        isAttacking = false;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
            animator.ResetTrigger("Attack");
            //animator.Play("Idle", 0, 0);
        }
    }

    void FindNearestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        float closestDistance = attackRange;
        Enemy newTarget = null;

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    newTarget = enemy;
                }
            }
        }

        if (targetEnemy != newTarget)
        {
            waitingForEnemyAttack = false;
            enemyIsAttacking = false;
        }

        targetEnemy = newTarget;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = moveDirection.magnitude > 0.1f && !isFighting && !isAttacking && !IsDead();
        float targetAnimSpeed = 0f;

        if (isMoving)
        {
            targetAnimSpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 1f : 0.3f;
        }

        float newAnimSpeed = Mathf.Lerp(animator.GetFloat("Speed"), targetAnimSpeed, Time.deltaTime * 10f);
        animator.SetFloat("Speed", newAnimSpeed);
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsFighting", isFighting);
        animator.SetBool("IsAttacking", isAttacking);

        if (moveDirection.magnitude > 0.1f)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveZ", moveDirection.z);
        }
    }

    void Attack()
    {
        if (animator == null) return;
        if (targetEnemy == null || !targetEnemy.IsAlive) return;

        lastAttackTime = Time.time;
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
        currentAttackCoroutine = StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        waitingForEnemyAttack = true;
        animator.SetBool("IsAttacking", true);
        animator.SetTrigger("Attack");

        Debug.Log("Игрок начал атаку");

        if (targetEnemy != null && targetEnemy.IsAlive)
        {
            Vector3 directionToEnemy = targetEnemy.transform.position - transform.position;
            directionToEnemy.y = 0;
            if (directionToEnemy != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(directionToEnemy);
        }

        while (waitingForEnemyAttack && targetEnemy != null && targetEnemy.IsAlive)
        {
            if (targetEnemy == null || !targetEnemy.IsAlive)
            {
                Debug.Log("Враг умер до окончания атаки");
                FinishAttackEarly();
                yield break;
            }
            yield return null;
        }

        if (targetEnemy == null || !targetEnemy.IsAlive)
        {
            Debug.Log("Враг мёртв, бой не проводится");
            FinishAttackEarly();
            yield break;
        }

        Debug.Log("Враг закончил атаку (Animation Event) - начинаем расчёт боя!");

        if (combat != null)
        {
            StartCoroutine(FightSequence(targetEnemy));
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        currentAttackCoroutine = null;
        waitingForEnemyAttack = false;

        Debug.Log("Атака игрока завершена");
    }

    void UpdatePlayerHeight()
    {
        // Бросаем луч от центра объекта вниз
        Vector3 rayStart = transform.position;
        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 0f, groundLayerMask))
        {
            // Прижимаем строго к поверхности земли (без дополнительного сдвига)
            float targetHeight = hit.point.y;
            float newY = Mathf.MoveTowards(transform.position.y, targetHeight, 100f * Time.deltaTime);
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        bool canMoveNow = canMove && !isFighting && !isAttacking && !IsDead() && moveDirection.magnitude > 0.1f;

        if (canMoveNow)
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;

            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    IEnumerator FightSequence(Enemy enemy)
    {
        if (isFighting) yield break;
        isFighting = true;

        if (animator != null) animator.SetBool("IsFighting", true);

        combat.FightEnemy(enemy);

        yield return new WaitForSeconds(0.3f);

        isFighting = false;
        if (animator != null) animator.SetBool("IsFighting", false);
    }

    public void Die()
    {
        if (animator != null) animator.SetBool("IsDead", true);
        canMove = false;
        isFighting = false;
        isAttacking = false;
        waitingForEnemyAttack = false;
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
    }

    public void Revive()
    {
        if (animator != null) animator.SetBool("IsDead", false);
        canMove = true;
    }

    private bool IsDead()
    {
        return animator != null && animator.GetBool("IsDead");
    }

    public void SetCanMoveAfterVictory(bool can)
    {
        canMoveAfterVictory = can;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}