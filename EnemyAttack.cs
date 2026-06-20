using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private enum AttackState
    {
        Ready,
        Windup,
        Active,
        Recovery
    }

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackRange = 2.2f;
    public float attackCooldown = 1.4f;

    [Header("Attack 1 Timing")]
    public float windupDuration = 0.63f;
    public float activeDuration = 0.100f;
    public float recoveryDuration = 1.33f;

    [Header("Follow Up Attack")]
    public bool useFollowUpAttack = true;
    [Range(0f, 1f)]
    public float followUpAttackChance = 0.45f;
    public int maxAttacksInSequence = 2;
    public float followUpWindupDuration = 0.833f;
    public float followUpActiveDuration = 0.133f;
    public float followUpRecoveryDuration = 1.367f;
    public float followUpRangeBonus = 0.4f;

    [Header("Hit Box")]
    public Transform attackHitBoxCenter;
    public Vector3 attackHitBoxHalfExtents = new Vector3(0.7f, 0.8f, 0.8f);
    public float hitBoxForwardOffset = 1.0f;
    public float hitBoxHeightOffset = 1.0f;
    public LayerMask playerLayer;

    private AttackState attackState = AttackState.Ready;

    private Transform currentTarget;
    private float stateTimer;
    private float cooldownTimer;
    private bool hasHitDuringThisAttack;

    private int attacksInCurrentSequence;
    private EnemySimpleAI enemySimpleAI;
    private EnemyAttackCoordinator attackCoordinator;
    public bool IsAttacking
    {
        get
        {
            return attackState == AttackState.Windup ||
                   attackState == AttackState.Active ||
                   attackState == AttackState.Recovery;
        }
    }

    public int AttackStartSequence { get; private set; }

    public int CurrentAttackStep
    {
        get { return attacksInCurrentSequence; }
    }

    private void Awake()
    {
        enemySimpleAI = GetComponent<EnemySimpleAI>();
        attackCoordinator = FindAnyObjectByType<EnemyAttackCoordinator>();
    }
    private void Update()
    {
        UpdateCooldown();

        switch (attackState)
        {
            case AttackState.Windup:
                UpdateWindup();
                break;

            case AttackState.Active:
                UpdateActive();
                break;

            case AttackState.Recovery:
                UpdateRecovery();
                break;
        }
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public bool CanAttack(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        if (IsAttacking)
        {
            return false;
        }

        if (cooldownTimer > 0f)
        {
            return false;
        }

        if (attackCoordinator != null && !attackCoordinator.CanRequestAttack(this))
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        return distance <= attackRange;
    }

    public void StartAttack(Transform target)
    {
        if (!CanAttack(target))
        {
            return;
        }

        if (attackCoordinator != null && !attackCoordinator.RequestAttackSlot(this))
        {
            return;
        }

        currentTarget = target;
        attackState = AttackState.Windup;
        stateTimer = windupDuration;
        hasHitDuringThisAttack = false;
        attacksInCurrentSequence = 1;
        AttackStartSequence++;

        FaceTarget();

        Debug.Log($"{gameObject.name} 공격 준비");
    }

    private void UpdateWindup()
    {
        stateTimer -= Time.deltaTime;

        FaceTarget();

        if (stateTimer <= 0f)
        {
            attackState = AttackState.Active;

            if (attacksInCurrentSequence >= 2)
            {
                stateTimer = followUpActiveDuration;
            }
            else
            {
                stateTimer = activeDuration;
            }

            Debug.Log($"{gameObject.name} 공격 판정 시작");
        }
    }

    private void UpdateActive()
    {
        stateTimer -= Time.deltaTime;

        FaceTarget();

        if (!hasHitDuringThisAttack)
        {
            TryHitPlayer();
        }

        if (stateTimer <= 0f)
        {
            attackState = AttackState.Recovery;

            if (attacksInCurrentSequence >= 2)
            {
                stateTimer = followUpRecoveryDuration;
            }
            else
            {
                stateTimer = recoveryDuration;
            }

            Debug.Log($"{gameObject.name} 공격 후딜");
        }
    }

    private void UpdateRecovery()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            EndAttack();
        }
    }

    private void EndAttack()
    {
        if (ShouldStartFollowUpAttack())
        {
            StartFollowUpAttack();
            return;
        }

        attackState = AttackState.Ready;
        cooldownTimer = attackCooldown;
        currentTarget = null;
        attacksInCurrentSequence = 0;

        Debug.Log($"{gameObject.name} 공격 종료");

        if (attackCoordinator != null)
        {
            attackCoordinator.ReleaseAttackSlot(this);
        }

        if (enemySimpleAI != null)
        {
            enemySimpleAI.DecideAfterAttack();
        }
    }

    private bool ShouldStartFollowUpAttack()
    {
        if (!useFollowUpAttack)
        {
            return false;
        }

        if (currentTarget == null)
        {
            return false;
        }

        if (attacksInCurrentSequence >= maxAttacksInSequence)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange + followUpRangeBonus)
        {
            return false;
        }

        return Random.value <= followUpAttackChance;
    }

    private void StartFollowUpAttack()
    {
        attacksInCurrentSequence++;
        AttackStartSequence++;

        attackState = AttackState.Windup;
        stateTimer = followUpWindupDuration;
        hasHitDuringThisAttack = false;

        FaceTarget();

        Debug.Log($"{gameObject.name} 연격 준비: {attacksInCurrentSequence}타");
    }

    private void TryHitPlayer()
    {
        Vector3 boxCenter = GetHitBoxCenter();
        Quaternion boxRotation = transform.rotation;

        Collider[] hitColliders = Physics.OverlapBox(
            boxCenter,
            attackHitBoxHalfExtents,
            boxRotation,
            playerLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hitCollider in hitColliders)
        {
            PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null)
            {
                continue;
            }

            hasHitDuringThisAttack = true;

            PlayerDamageResult damageResult = playerHealth.TakeDamage(attackDamage, transform.position);

            switch (damageResult)
            {
                case PlayerDamageResult.Damaged:
                    Debug.Log($"{gameObject.name} 공격 명중! 데미지: {attackDamage}");
                    break;

                case PlayerDamageResult.Blocked:
                    Debug.Log($"{gameObject.name} 공격이 막힘");
                    break;

                case PlayerDamageResult.Dodged:
                    Debug.Log($"{gameObject.name} 공격을 구르기로 회피");
                    break;

                case PlayerDamageResult.Cooldown:
                    Debug.Log($"{gameObject.name} 공격은 닿았지만 피격 쿨다운으로 무시됨");
                    break;

                default:
                    Debug.Log($"{gameObject.name} 공격은 닿았지만 데미지는 들어가지 않음");
                    break;
            }

            return;
        }
    }

    private Vector3 GetHitBoxCenter()
    {
        if (attackHitBoxCenter != null)
        {
            return attackHitBoxCenter.position;
        }

        return transform.position +
               Vector3.up * hitBoxHeightOffset +
               transform.forward * hitBoxForwardOffset;
    }

    private void FaceTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void InterruptAttack(float cooldownAfterInterrupt)
    {
        if (!IsAttacking)
        {
            return;
        }

        attackState = AttackState.Ready;
        cooldownTimer = Mathf.Max(cooldownTimer, cooldownAfterInterrupt);

        currentTarget = null;
        hasHitDuringThisAttack = false;
        attacksInCurrentSequence = 0;

        if (attackCoordinator != null)
        {
            attackCoordinator.ReleaseAttackSlot(this);
        }

        Debug.Log($"{gameObject.name} 피격으로 공격 중단");
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 boxCenter;

        if (attackHitBoxCenter != null)
        {
            boxCenter = attackHitBoxCenter.position;
        }
        else
        {
            boxCenter =
                transform.position +
                Vector3.up * hitBoxHeightOffset +
                transform.forward * hitBoxForwardOffset;
        }

        if (attackState == AttackState.Active)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            boxCenter,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, attackHitBoxHalfExtents * 2f);

        Gizmos.matrix = oldMatrix;
    }

    private void OnDisable()
    {
        if (attackCoordinator != null)
        {
            attackCoordinator.ReleaseAttackSlot(this);
        }
    }
}