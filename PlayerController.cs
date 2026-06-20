using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Normal,
        Dodge,
        Block,
        Attack
    }

    [Header("Lock On Movement")]
    public LockOnSystem lockOnSystem;
    public bool useLockOnMovement = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 9f;
    public float turnSpeed = 12f;
    public Transform cameraTransform;

    [Header("Dodge")]
    public float dodgeSpeed = 12f;
    public float dodgeDuration = 1.10f;

    [Header("Dodge Invincibility")]
    public float dodgeInvincibleStartTime = 0.10f;
    public float dodgeInvincibleEndTime = 1.03f;

    [Header("Corpse Shield")]
    public CorpseShield corpseShield;
    public float corpseShieldRunSpeed = 7f;
    public bool IsInvincible { get; private set; }
    public bool IsBlocking
    {
        get
        {
            return currentState == PlayerState.Block;
        }
    }

    public bool IsAttacking
    {
        get
        {
            return currentState == PlayerState.Attack;
        }
    }
    public int CurrentComboStep
    {
        get { return comboStep; }
    }

    public bool IsSprinting
    {
        get
        {
            bool isHoldingCorpse =
                corpseShield != null &&
                corpseShield.IsHoldingCorpse;

            return !isExternalActionLocked &&
                   currentState == PlayerState.Normal &&
                   !isHoldingCorpse &&
                   moveInput != Vector3.zero &&
                   Input.GetKey(KeyCode.LeftShift) &&
                   !Input.GetMouseButton(1);
        }
    }

    public bool IsCorpseRunning
    {
        get
        {
            bool isHoldingCorpse =
                corpseShield != null &&
                corpseShield.IsHoldingCorpse;

            return !isExternalActionLocked &&
                   currentState == PlayerState.Normal &&
                   isHoldingCorpse &&
                   moveInput != Vector3.zero &&
                   Input.GetKey(KeyCode.LeftShift);
        }
    }

    public int AttackStartSequence { get; private set; }
    public int DodgeStartSequence { get; private set; }

    private float dodgeElapsedTime;

    [Header("Block")]
    public float blockMoveSpeedMultiplier = 0.35f;

    [Header("Attack")]
    public Transform weaponHitBoxCenter;
    public Vector3 weaponHitBoxHalfExtents = new Vector3(0.25f, 0.25f, 1.2f);
    public LayerMask enemyLayer;

    [Header("Combo")]
    public float comboInputStartTime = 0.08f;

    private Rigidbody rb;

    private Vector3 moveInput;
    private Vector3 moveDirection;
    private Vector3 dodgeDirection;

    private PlayerState currentState = PlayerState.Normal;
    private float stateTimer;
    private float attackElapsedTime;

    private int comboStep = 0;
    private bool hasHitDuringThisAttack;
    private bool queuedNextAttack;

    private bool isExternalActionLocked;

    public bool CanStartExternalSkill()
    {
        if (corpseShield != null && corpseShield.IsHoldingCorpse)
        {
            return false;
        }

        return !isExternalActionLocked &&
               (currentState == PlayerState.Normal ||
                currentState == PlayerState.Block);
    }

    public void SetExternalActionLock(bool isLocked)
    {
        isExternalActionLocked = isLocked;

        if (isLocked)
        {
            currentState = PlayerState.Normal;
            comboStep = 0;
            queuedNextAttack = false;
            attackElapsedTime = 0f;
            dodgeElapsedTime = 0f;
            IsInvincible = false;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("PlayerController가 붙은 오브젝트에 Rigidbody가 없습니다.");
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (lockOnSystem == null)
        {
            lockOnSystem = GetComponent<LockOnSystem>();
        }

        if (corpseShield == null)
        {
            corpseShield = GetComponent<CorpseShield>();
        }
    }

    private void Update()
    {
        if (isExternalActionLocked)
        {
            return;
        }

        ReadMoveInput();
        CalculateMoveDirection();

        UpdateStateTimer();
        HandleActionInput();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.angularVelocity = Vector3.zero;

        if (isExternalActionLocked)
        {
            return;
        }

        HandleMovement();
        HandleRotationPhysics();
    }

    private void ReadMoveInput()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.W)) z += 1f;

        moveInput = new Vector3(x, 0f, z).normalized;
    }

    private void CalculateMoveDirection()
    {
        if (HasLockOnTarget())
        {
            CalculateLockOnMoveDirection();
            return;
        }

        CalculateCameraRelativeMoveDirection();
    }

    private void CalculateCameraRelativeMoveDirection()
    {
        if (cameraTransform == null)
        {
            moveDirection = moveInput;
            return;
        }

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = cameraForward * moveInput.z + cameraRight * moveInput.x;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }

    private void CalculateLockOnMoveDirection()
    {
        if (moveInput == Vector3.zero)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 directionToTarget = GetFlatDirectionToLockTarget();

        if (directionToTarget == Vector3.zero)
        {
            CalculateCameraRelativeMoveDirection();
            return;
        }

        Vector3 rightDirection = Vector3.Cross(Vector3.up, directionToTarget).normalized;

        moveDirection = directionToTarget * moveInput.z + rightDirection * moveInput.x;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }
    private bool HasLockOnTarget()
    {
        return useLockOnMovement &&
               lockOnSystem != null &&
               lockOnSystem.CurrentTargetTransform != null;
    }

    private Vector3 GetFlatDirectionToLockTarget()
    {
        if (lockOnSystem == null || lockOnSystem.CurrentTargetTransform == null)
        {
            return Vector3.zero;
        }

        Vector3 direction = lockOnSystem.CurrentTargetTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return Vector3.zero;
        }

        direction.Normalize();
        return direction;
    }

    private void UpdateStateTimer()
    {
        if (currentState != PlayerState.Dodge && currentState != PlayerState.Attack)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        if (currentState == PlayerState.Dodge)
        {
            dodgeElapsedTime += Time.deltaTime;
            UpdateDodgeInvincibility();
        }

        if (currentState == PlayerState.Attack)
        {
            attackElapsedTime += Time.deltaTime;

            if (!hasHitDuringThisAttack && IsAttackHitWindowActive())
            {
                TryAttackHit();
            }
        }

        if (stateTimer <= 0f)
        {
            if (currentState == PlayerState.Attack && queuedNextAttack && comboStep < 3)
            {
                StartAttack(comboStep + 1);
                return;
            }

            currentState = PlayerState.Normal;
            comboStep = 0;
            queuedNextAttack = false;
            attackElapsedTime = 0f;
        }
    }

    private void UpdateDodgeInvincibility()
    {
        bool shouldBeInvincible =
            currentState == PlayerState.Dodge &&
            dodgeElapsedTime >= dodgeInvincibleStartTime &&
            dodgeElapsedTime <= dodgeInvincibleEndTime;

        if (IsInvincible == shouldBeInvincible)
        {
            return;
        }

        IsInvincible = shouldBeInvincible;

        if (IsInvincible)
        {
            Debug.Log("구르기 무적 시작");
        }
        else
        {
            Debug.Log("구르기 무적 종료");
        }
    }

    private void HandleActionInput()
    {
        if (isExternalActionLocked)
        {
            return;
        }

        if (currentState == PlayerState.Dodge)
        {
            return;
        }

        if (currentState == PlayerState.Attack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (comboStep < 3)
                {
                    queuedNextAttack = true;
                    Debug.Log($"{comboStep + 1}타 예약");
                }
            }

            return;
        }

        bool isHoldingCorpse =
    corpseShield != null &&
    corpseShield.IsHoldingCorpse;

        if (isHoldingCorpse)
        {
            // 좌클릭 공격은 허용한다.
            // 대신 공격하는 순간 시신 방패는 소모된다.
            if (Input.GetMouseButtonDown(0))
            {
                corpseShield.ConsumeForPlayerAttack();
                StartAttack(1);
                return;
            }

            // 시신을 들고 있는 동안 구르기 금지.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("시신 방패 중에는 구르기 불가");
                return;
            }

            // 시신을 들고 있는 동안 일반 막기 금지.
            if (Input.GetMouseButton(1))
            {
                currentState = PlayerState.Normal;
                Debug.Log("시신 방패 중에는 일반 막기 불가");
                return;
            }

            currentState = PlayerState.Normal;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartDodge();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartAttack(1);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            currentState = PlayerState.Block;
            return;
        }

        currentState = PlayerState.Normal;
    }

    private void StartDodge()
    {
        currentState = PlayerState.Dodge;
        DodgeStartSequence++;

        stateTimer = dodgeDuration;
        dodgeElapsedTime = 0f;
        IsInvincible = false;

        comboStep = 0;
        queuedNextAttack = false;
        attackElapsedTime = 0f;

        if (moveDirection != Vector3.zero)
        {
            dodgeDirection = moveDirection;
        }
        else
        {
            // 방향키가 없으면 락온 여부와 관계없이
            // 현재 캐릭터가 바라보는 방향으로 구른다.
            dodgeDirection = transform.forward;
        }

        rb.angularVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.LookRotation(dodgeDirection));
    }

    private void StartAttack(int nextComboStep)
    {
        currentState = PlayerState.Attack;
        comboStep = nextComboStep;

        AttackStartSequence++;

        stateTimer = GetAttackDuration(comboStep);
        attackElapsedTime = 0f;

        hasHitDuringThisAttack = false;
        queuedNextAttack = false;

        Vector3 attackDirection = GetAttackDirection();

        if (attackDirection != Vector3.zero)
        {
            rb.angularVelocity = Vector3.zero;
            rb.MoveRotation(Quaternion.LookRotation(attackDirection));
        }

        Debug.Log($"{comboStep}타 공격 시작");
    }

    private Vector3 GetAttackDirection()
    {
        if (HasLockOnTarget())
        {
            return GetFlatDirectionToLockTarget();
        }

        if (moveDirection != Vector3.zero)
        {
            return moveDirection;
        }

        return transform.forward;
    }

    private float GetAttackDuration(int step)
    {
        switch (step)
        {
            case 1:
                return 1.10f;
            case 2:
                return 0.72f;
            case 3:
                return 1.33f;
            default:
                return 0.65f;
        }
    }

    private float GetAttackHitStartTime(int step)
    {
        switch (step)
        {
            case 1:
                return 0.30f;
            case 2:
                return 0.15f;
            case 3:
                return 0.53f;
            default:
                return 0.18f;
        }
    }

    private float GetAttackHitEndTime(int step)
    {
        switch (step)
        {
            case 1:
                return 0.67f;
            case 2:
                return 0.53f;
            case 3:
                return 0.87f;
            default:
                return 0.28f;
        }
    }

    private bool IsAttackHitWindowActive()
    {
        if (currentState != PlayerState.Attack)
        {
            return false;
        }

        float hitStartTime = GetAttackHitStartTime(comboStep);
        float hitEndTime = GetAttackHitEndTime(comboStep);

        return attackElapsedTime >= hitStartTime && attackElapsedTime <= hitEndTime;
    }

    private int GetAttackDamage(int step)
    {
        switch (step)
        {
            case 1:
                return 20;
            case 2:
                return 25;
            case 3:
                return 35;
            default:
                return 20;
        }
    }

    private void HandleMovement()
    {
        switch (currentState)
        {
            case PlayerState.Dodge:
                MoveDuringDodge();
                break;

            case PlayerState.Attack:
                StopHorizontalMovement();
                break;

            case PlayerState.Block:
                MoveNormally(moveSpeed * blockMoveSpeedMultiplier);
                break;

            case PlayerState.Normal:
                MoveNormally(GetCurrentMoveSpeed());
                break;
        }
    }

    private float GetCurrentMoveSpeed()
    {
        bool isHoldingCorpse =
            corpseShield != null &&
            corpseShield.IsHoldingCorpse;

        if (isHoldingCorpse)
        {
            // 시신을 들어도 기본 걷기 속도는 유지.
            // Shift를 누르면 평소 달리기보다는 느린 전용 달리기.
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return corpseShieldRunSpeed;
            }

            return moveSpeed;
        }

        if (Input.GetKey(KeyCode.LeftShift) &&
            !Input.GetMouseButton(1))
        {
            return runSpeed;
        }

        return moveSpeed;
    }
    private void HandleRotationPhysics()
    {
        if (currentState == PlayerState.Dodge ||
        currentState == PlayerState.Attack)
        {
            return;
        }
        // 아래의 기존 Sprint / LockOn / 일반 회전 코드는 그대로 둔다.

        // Sprint 중에는 락온을 유지하더라도
        // 몸은 실제 달리는 방향을 바라본다.
        if (IsSprinting && moveDirection != Vector3.zero)
        {
            RotateTowardDirection(moveDirection);
            return;
        }

        // Sprint가 끝나면 기존 락온 방향 규칙으로 돌아간다.
        if (HasLockOnTarget())
        {
            RotateTowardLockOnTarget();
            return;
        }

        if (moveDirection == Vector3.zero)
        {
            return;
        }

        RotateTowardDirection(moveDirection);
    }

    private void RotateTowardLockOnTarget()
    {
        Vector3 targetDirection = GetFlatDirectionToLockTarget();

        if (targetDirection == Vector3.zero)
        {
            return;
        }

        RotateTowardDirection(targetDirection);
    }

    private void RotateTowardDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.y = 0f;
        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Quaternion nextRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    private void MoveNormally(float speed)
    {
        Vector3 moveVelocity = moveDirection * speed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
    }

    private void MoveDuringDodge()
    {
        Vector3 dodgeVelocity = dodgeDirection * dodgeSpeed;
        rb.linearVelocity = new Vector3(dodgeVelocity.x, rb.linearVelocity.y, dodgeVelocity.z);
    }

    private void StopHorizontalMovement()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    private void TryAttackHit()
    { 
        if (weaponHitBoxCenter == null)
        {
            Debug.LogWarning("WeaponHitBoxCenter가 비어 있습니다.");
            return;
        }

        Collider[] hitColliders = Physics.OverlapBox(
            weaponHitBoxCenter.position,
            weaponHitBoxHalfExtents,
            weaponHitBoxCenter.rotation,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (Collider hitCollider in hitColliders)
        {
            EnemyHealth enemyHealth = null;

            EnemyHitBox enemyHitBox = hitCollider.GetComponent<EnemyHitBox>();

            if (enemyHitBox != null)
            {
                enemyHealth = enemyHitBox.GetEnemyHealth();
            }
            else
            {
                enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth == null)
            {
                continue;
            }

            if (damagedEnemies.Contains(enemyHealth))
            {
                continue;
            }

            damagedEnemies.Add(enemyHealth);

            hasHitDuringThisAttack = true;
            
            int damage = GetAttackDamage(comboStep);

            if (enemyHitBox != null)
            {
                enemyHitBox.TakeHit(damage, transform.position);
            }
            else
            {
                enemyHealth.TakeDamage(damage, transform.position);
            }

            Debug.Log($"{comboStep}타 명중! 데미지: {damage}");
        }
    }

    private void OnDrawGizmos()
    {
        if (weaponHitBoxCenter == null)
        {
            return;
        }

        if (Application.isPlaying && IsAttackHitWindowActive())
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            weaponHitBoxCenter.position,
            weaponHitBoxCenter.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, weaponHitBoxHalfExtents * 2f);

        Gizmos.matrix = oldMatrix;
    }
}