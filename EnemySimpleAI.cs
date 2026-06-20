using UnityEngine;

public class EnemySimpleAI : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        AlertStare,      // 원거리 발견/응시
        AlertCircle,     // 원거리 선회
        Approach,        // 추격/압박
        ApproachAttack,  // 공격 범위까지 더 들어가서 바로 공격
        CloseFaceOff,    // 근거리 대치
        CloseCircle,     // 근거리 선회
        Backstep,         // 근거리에서 한 번 후퇴
        Guard,
        Flee,
        Return
    }

    [Header("Target")]
    public Transform target;

    [Header("Detection")]
    public float detectDistance = 12f;
    public float loseDistance = 18f;

    [Header("Far Alert Zone")]
    public float alertMinDistance = 8f;
    public float alertMaxDistance = 12f;
    public float alertStareMinDuration = 1f;
    public float alertStareMaxDuration = 2f;
    public float alertCircleChance = 0.6f;
    public float alertPatrolChance = 0.25f;

    [Header("Approach")]
    public float approachSpeed = 2.8f;
    public float closeFaceOffDistance = 4.5f;

    [Header("Approach Decision")]
    public float approachAttackChance = 0.45f;
    public float approachAttackSpeed = 3.2f;
    public float approachAttackTimeout = 1.4f;

    [Header("Close Face Off")]
    public float closeFaceOffBreakDistance = 6f;
    public float closeDecisionInterval = 1.2f;
    public float closeCircleChance = 0.6f;

    [Header("Post Attack Decision")]
    public bool usePostAttackDecision = true;
    public float postAttackFaceOffChance = 0.45f;
    public float postAttackCircleChance = 0.35f;
    public float postAttackFleeChance = 0.2f;

    [Header("Flee")]
    public float fleeDistance = 2.2f;
    public float fleeRecoverDistance = 3.5f;
    public float fleeSpeed = 3.5f;

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float returnSpeed = 3f;
    public float turnSpeed = 10f;

    [Header("Patrol")]
    public float patrolRadius = 4f;
    public float patrolPointReachDistance = 0.4f;
    public float patrolWaitDuration = 1.2f;

    [Header("Circle")]
    public float alertCircleSpeed = 1.4f;
    public float closeCircleSpeed = 1.8f;
    public float alertCircleMinDuration = 1.5f;
    public float alertCircleMaxDuration = 3.2f;
    public float closeCircleMinDuration = 1.2f;
    public float closeCircleMaxDuration = 2.5f;
    public float circleDistanceCorrection = 0.8f;

    [Header("Guarded Close Circle")]
    public bool useGuardedCloseCircle = true;

    [Range(0f, 1f)]
    public float guardedCloseCircleChance = 0.35f;

    [Header("Limited Backstep Evasion")]
    public bool useBackstepEvasion = true;


    [Range(0f, 1f)]
    public float backstepChance = 0.18f;

    public float backstepTriggerDistance = 3.2f;
    public float backstepSpeed = 5.5f;
    public float backstepDuration = 0.22f;
    public float backstepCooldown = 3.5f;

    [Header("Limited Guard Defense")]
    public bool useGuardDefense = true;

    [Range(0f, 1f)]
    public float guardChance = 0.22f;

    [Range(0.05f, 1f)]
    public float guardDamageMultiplier = 0.5f;

    public float guardTriggerDistance = 3.4f;
    public float guardDuration = 0.45f;
    public float guardCooldown = 4f;

    [Header("Hit Stun")]
    public float hitStunDuration = 0.25f;

    // 디버그용으로, 시작하자마자 방어 상태로 시작하거나 방어 상태를 계속 유지하도록 하는 옵션.
    [Header("Guard Test")]
    public bool startInGuardForTest = false;
    public bool holdGuardForTest = false;

    private Rigidbody rb;
    private EnemyState currentState = EnemyState.Patrol;
    private EnemyAttack enemyAttack;

    private Vector3 startPosition;
    private Vector3 patrolTargetPosition;

    private float waitTimer;
    private float alertTimer;
    private float closeDecisionTimer;
    private float circleTimer;

    private int circleDirection = 1;

    private float hitStunTimer;

    private float approachAttackTimer;

    private float backstepTimer;
    private float backstepCooldownTimer;
    private Vector3 backstepDirection;

    private float guardTimer;
    private float guardCooldownTimer;
    private bool isGuardMoving;

    private int lastObservedAttackSequence;
    private PlayerController targetPlayerController;

    public bool IsGuarding
    {
        get
        {
            return currentState == EnemyState.Guard ||
                   isGuardMoving;
        }
    }

    public float GuardDamageMultiplier
    {
        get { return guardDamageMultiplier; }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        enemyAttack = GetComponent<EnemyAttack>();

        if (target != null)
        {
            targetPlayerController = target.GetComponentInParent<PlayerController>();

            if (targetPlayerController != null)
            {
                lastObservedAttackSequence =
                    targetPlayerController.AttackStartSequence;
            }
        }

        if (rb == null)
        {
            Debug.LogError("EnemySimpleAI가 붙은 오브젝트에 Rigidbody가 없습니다.");
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        startPosition = transform.position;
        PickNewPatrolPoint();
    }
    private void Start()
    {
        if (startInGuardForTest)
        {
            ChangeState(EnemyState.Guard);
            Debug.Log($"{gameObject.name} 테스트용 Guard 시작");
        }
    } // 돌파 가드브레이크 테스트를 위해, 시작하자마자 방어 상태로 들어가는 옵션.

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.angularVelocity = Vector3.zero;

        UpdateBackstepCooldown();
        UpdateGuardCooldown();

        bool playerAttackJustStarted = CheckPlayerAttackJustStarted();

        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.fixedDeltaTime;
            return;
        }

        if (enemyAttack != null && enemyAttack.IsAttacking)
        {
            StopMoving();

            if (target != null)
            {
                FaceTarget(target.position);
            }

            return;
        }

        if (playerAttackJustStarted && TryReactToPlayerAttack())
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.AlertStare:
                UpdateAlertStare();
                break;

            case EnemyState.AlertCircle:
                UpdateAlertCircle();
                break;

            case EnemyState.Approach:
                UpdateApproach();
                break;

            case EnemyState.ApproachAttack:
                UpdateApproachAttack();
                break;

            case EnemyState.CloseFaceOff:
                UpdateCloseFaceOff();
                break;

            case EnemyState.CloseCircle:
                UpdateCloseCircle();
                break;

            case EnemyState.Backstep:
                UpdateBackstep();
                break;

            case EnemyState.Guard:
                UpdateGuard();
                break;

            case EnemyState.Flee:
                UpdateFlee();
                break;

            case EnemyState.Return:
                UpdateReturn();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (CanDetectTarget())
        {
            ChangeState(EnemyState.AlertStare);
            return;
        }

        float distanceToPatrolPoint = Vector3.Distance(transform.position, patrolTargetPosition);

        if (distanceToPatrolPoint <= patrolPointReachDistance)
        {
            StopMoving();

            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
            {
                PickNewPatrolPoint();
            }

            return;
        }

        MoveToward(patrolTargetPosition, patrolSpeed);
    }

    private void UpdateAlertStare()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        FaceTarget(target.position);
        StopMoving();

        // 원거리 대치권 밖으로 완전히 벗어나면 관심을 끊음.
        if (distance > alertMaxDistance)
        {
            PickNewPatrolPoint();
            ChangeState(EnemyState.Patrol);
            return;
        }

        // 플레이어가 원거리 대치권보다 확실히 가까이 들어오면 추격/압박 시작.
        if (distance < alertMinDistance)
        {
            ChangeState(EnemyState.Approach);
            return;
        }

        // alertMinDistance ~ alertMaxDistance 안에서는
        // 플레이어가 와리가리해도 계속 응시한다.
        alertTimer -= Time.fixedDeltaTime;

        if (alertTimer > 0f)
        {
            return;
        }

        DecideAfterAlertStare();
    }

    private void DecideAfterAlertStare()
    {
        float randomValue = Random.value;

        if (randomValue <= alertCircleChance)
        {
            ChangeState(EnemyState.AlertCircle);
            return;
        }

        if (randomValue <= alertCircleChance + alertPatrolChance)
        {
            PickNewPatrolPoint();
            ChangeState(EnemyState.Patrol);
            return;
        }

        ChangeState(EnemyState.AlertStare);
    }

    private void UpdateAlertCircle()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        // 원거리 대치권 밖으로 벗어나면 관심 끊고 순찰.
        if (distance > alertMaxDistance)
        {
            PickNewPatrolPoint();
            ChangeState(EnemyState.Patrol);
            return;
        }

        // 플레이어가 가까이 들어오면 추격/압박.
        if (distance < alertMinDistance)
        {
            ChangeState(EnemyState.Approach);
            return;
        }

        CircleAroundTarget(GetAlertCircleDistance(), alertCircleSpeed);

        circleTimer -= Time.fixedDeltaTime;

        if (circleTimer <= 0f)
        {
            ChangeState(EnemyState.AlertStare);
        }
    }

    private void UpdateApproach()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        // 달리기/돌파 등으로 확실히 멀어졌을 때만 추격 포기.
        if (distance > loseDistance)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        // 너무 붙으면 후퇴.
        if (distance <= fleeDistance)
        {
            ChangeState(EnemyState.Flee);
            return;
        }

        // 근거리까지 들어오면,
        // 그냥 대치할지 / 공격 범위까지 더 들어가서 바로 칠지 고른다.
        if (distance <= closeFaceOffDistance)
        {
            DecideAfterApproach();
            return;
        }

        MoveToward(target.position, approachSpeed);
    }

    private void DecideAfterApproach()
    {
        if (enemyAttack != null && Random.value <= approachAttackChance)
        {
            Debug.Log($"{gameObject.name} 접근 후 바로 공격 시도");
            ChangeState(EnemyState.ApproachAttack);
            return;
        }

        ChangeState(EnemyState.CloseFaceOff);
    }

    private void UpdateApproachAttack()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        if (distance > loseDistance)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        FaceTarget(target.position);

        if (enemyAttack != null && enemyAttack.CanAttack(target))
        {
            StopMoving();
            enemyAttack.StartAttack(target);
            return;
        }

        approachAttackTimer -= Time.fixedDeltaTime;

        if (approachAttackTimer <= 0f)
        {
            ChangeState(EnemyState.CloseFaceOff);
            return;
        }

        // 공격 범위에 못 들어갔는데 너무 붙어버린 경우 안전하게 후퇴.
        // 단, 위에서 CanAttack을 먼저 검사하므로 공격 가능한 거리면 공격이 우선된다.
        if (distance <= fleeDistance * 0.8f)
        {
            ChangeState(EnemyState.Flee);
            return;
        }

        MoveToward(target.position, approachAttackSpeed);
    }

    private void UpdateCloseFaceOff()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        FaceTarget(target.position);
        StopMoving();

        if (distance > loseDistance)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        // 플레이어가 빠져나가면 다시 추격.
        if (distance > closeFaceOffBreakDistance)
        {
            ChangeState(EnemyState.Approach);
            return;
        }

        // 너무 붙으면 후퇴.
        if (distance <= fleeDistance)
        {
            ChangeState(EnemyState.Flee);
            return;
        }

        if (enemyAttack != null && enemyAttack.CanAttack(target))
        {
            enemyAttack.StartAttack(target);
            return;
        }

        closeDecisionTimer -= Time.fixedDeltaTime;

        if (closeDecisionTimer > 0f)
        {
            return;
        }

        if (Random.value <= closeCircleChance)
        {
            ChangeState(EnemyState.CloseCircle);
            return;
        }

        closeDecisionTimer = closeDecisionInterval;
    }

    private void UpdateCloseCircle()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        if (distance > loseDistance)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        if (distance > closeFaceOffBreakDistance)
        {
            ChangeState(EnemyState.Approach);
            return;
        }

        if (distance <= fleeDistance)
        {
            ChangeState(EnemyState.Flee);
            return;
        }

        if (enemyAttack != null && enemyAttack.CanAttack(target))
        {
            enemyAttack.StartAttack(target);
            return;
        }

        CircleAroundTarget(GetCloseCircleDistance(), closeCircleSpeed);

        circleTimer -= Time.fixedDeltaTime;

        if (circleTimer <= 0f)
        {
            ChangeState(EnemyState.CloseFaceOff);
        }
    }

    private void UpdateBackstepCooldown()
{
    if (backstepCooldownTimer > 0f)
    {
        backstepCooldownTimer -= Time.fixedDeltaTime;
    }
}

    private bool CheckPlayerAttackJustStarted()
    {
        if (target == null)
        {
            return false;
        }

        if (targetPlayerController == null)
        {
            targetPlayerController =
                target.GetComponentInParent<PlayerController>();
        }

        if (targetPlayerController == null)
        {
            return false;
        }

        int currentAttackSequence =
            targetPlayerController.AttackStartSequence;

        if (currentAttackSequence == lastObservedAttackSequence)
        {
            return false;
        }

        lastObservedAttackSequence = currentAttackSequence;

        Debug.Log(
            $"{gameObject.name} 플레이어의 새 공격 감지: {currentAttackSequence}"
        );

        return true;
    }

    private bool TryStartBackstep()
    {
    if (!useBackstepEvasion)
    {
        return false;
    }

    if (target == null)
    {
        return false;
    }

    if (backstepCooldownTimer > 0f)
    {
        return false;
    }

    if (!CanBackstepInCurrentState())
    {
        return false;
    }

    if (GetDistanceToTarget() > backstepTriggerDistance)
    {
        return false;
    }

    if (Random.value > backstepChance)
    {
        return false;
    }

    Debug.Log($"{gameObject.name} 플레이어 공격을 보고 백스텝 시도");

        StartBackstep();
        return true;
    }

    private bool TryReactToPlayerAttack()
    {
        if (IsGuarding)
        {
            return false;
        }
        // 먼저 백스텝을 시도한다.
        if (TryStartBackstep())
        {
            return true;
        }

        // 백스텝하지 않았다면 방어를 시도한다.
        return TryStartGuard();
    }

    private bool TryStartGuard()
    {
        if (!useGuardDefense)
        {
            return false;
        }

        if (target == null)
        {
            return false;
        }

        if (guardCooldownTimer > 0f)
        {
            return false;
        }

        if (!CanGuardInCurrentState())
        {
            return false;
        }

        if (GetDistanceToTarget() > guardTriggerDistance)
        {
            return false;
        }

        if (Random.value > guardChance)
        {
            return false;
        }

        Debug.Log($"{gameObject.name} 플레이어 공격을 보고 방어 시도");

        ChangeState(EnemyState.Guard);
        return true;
    }

    private bool CanGuardInCurrentState()
    {
        return currentState == EnemyState.Approach ||
               currentState == EnemyState.ApproachAttack ||
               currentState == EnemyState.CloseFaceOff ||
               currentState == EnemyState.CloseCircle;
    }

    private void UpdateGuard()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        StopMoving();
        FaceTarget(target.position);

        if (holdGuardForTest)
        {
            return;
        }

        guardTimer -= Time.fixedDeltaTime;

        if (guardTimer > 0f)
        {
            return;
        }

        Debug.Log($"{gameObject.name} 방어 종료");
        ChangeState(EnemyState.CloseFaceOff);
    }

    private void UpdateGuardCooldown()
    {
        if (guardCooldownTimer > 0f)
        {
            guardCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    public void ResolveGuardHit(bool brokenByCharge)
    {
        if (!IsGuarding)
        {
            return;
        }

        if (brokenByCharge)
        {
            Debug.Log($"{gameObject.name} 돌파에 방어가 깨짐");
        }
        else
        {
            Debug.Log($"{gameObject.name} 방어했지만 밀려남");
        }

        guardCooldownTimer = guardCooldown;
        ChangeState(EnemyState.CloseFaceOff);
    }

    private void StartBackstep()
    {
        if (target == null)
        {
            return;
        }

        backstepDirection = transform.position - target.position;
        backstepDirection.y = 0f;

        if (backstepDirection.sqrMagnitude <= 0.001f)
        {
            backstepDirection = -transform.forward;
        }

        backstepDirection.Normalize();

        ChangeState(EnemyState.Backstep);

        // 상태만 바꾸고 다음 FixedUpdate를 기다리지 않고,
        // 백스텝을 선택한 순간 바로 이동을 시작한다.
        ApplyBackstepVelocity();
    }

    private void ApplyBackstepVelocity()
    {
        rb.linearVelocity = new Vector3(
            backstepDirection.x * backstepSpeed,
            rb.linearVelocity.y,
            backstepDirection.z * backstepSpeed
        );

        if (target != null)
        {
            // 이동은 뒤로 하지만 몸은 계속 Player를 바라본다.
            FaceTarget(target.position);
        }
    }

    private bool CanBackstepInCurrentState()
    {
        return currentState == EnemyState.Approach ||
               currentState == EnemyState.ApproachAttack ||
               currentState == EnemyState.CloseFaceOff ||
               currentState == EnemyState.CloseCircle ||
               currentState == EnemyState.Flee;
    }

    private void UpdateBackstep()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        ApplyBackstepVelocity();

        backstepTimer -= Time.fixedDeltaTime;

        if (backstepTimer > 0f)
        {
            return;
        }

        StopMoving();

        float distance = GetDistanceToTarget();

        if (distance > closeFaceOffBreakDistance)
        {
            ChangeState(EnemyState.Approach);
            return;
        }

        ChangeState(EnemyState.CloseFaceOff);
    }

    public void PauseAI(float duration)
    {
        hitStunTimer = Mathf.Max(hitStunTimer, duration);

        Debug.Log($"{gameObject.name} 피격 경직");
    }
    public void DecideAfterAttack()
    {
        if (!usePostAttackDecision)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        float distance = GetDistanceToTarget();

        if (distance > loseDistance)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        if (distance > closeFaceOffBreakDistance)
        {
            ChangeState(EnemyState.Approach);
            return;
        }

        float totalChance =
            postAttackFaceOffChance +
            postAttackCircleChance +
            postAttackFleeChance;

        if (totalChance <= 0f)
        {
            ChangeState(EnemyState.CloseFaceOff);
            return;
        }

        float roll = Random.value * totalChance;

        if (roll < postAttackFleeChance)
        {
            Debug.Log($"{gameObject.name} 공격 후 뒤로 빠짐");
            ChangeState(EnemyState.Flee);
            return;
        }

        roll -= postAttackFleeChance;

        if (roll < postAttackCircleChance)
        {
            Debug.Log($"{gameObject.name} 공격 후 측면 선회");
            ChangeState(EnemyState.CloseCircle);
            return;
        }

        Debug.Log($"{gameObject.name} 공격 후 재대치");
        ChangeState(EnemyState.CloseFaceOff);
    }

    private void UpdateFlee()
    {
        if (target == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distance = GetDistanceToTarget();

        if (distance > loseDistance)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        if (distance >= fleeRecoverDistance)
        {
            ChangeState(EnemyState.CloseFaceOff);
            return;
        }

        MoveAwayFrom(target.position, fleeSpeed);
    }

    private void UpdateReturn()
    {
        float distanceToStart = Vector3.Distance(transform.position, startPosition);

        if (CanDetectTarget())
        {
            ChangeState(EnemyState.AlertStare);
            return;
        }

        if (distanceToStart <= patrolPointReachDistance)
        {
            StopMoving();
            PickNewPatrolPoint();
            ChangeState(EnemyState.Patrol);
            return;
        }

        MoveToward(startPosition, returnSpeed);
    }

    private bool CanDetectTarget()
    {
        if (target == null)
        {
            return false;
        }

        float distance = GetDistanceToTarget();
        return distance <= detectDistance;
    }

    private float GetDistanceToTarget()
    {
        if (target == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(transform.position, target.position);
    }

    private float GetAlertCircleDistance()
    {
        return (alertMinDistance + alertMaxDistance) * 0.5f;
    }

    private float GetCloseCircleDistance()
    {
        return (fleeRecoverDistance + closeFaceOffDistance) * 0.5f;
    }

    private void MoveToward(Vector3 destination, float speed)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            StopMoving();
            return;
        }

        direction.Normalize();

        rb.linearVelocity = new Vector3(
            direction.x * speed,
            rb.linearVelocity.y,
            direction.z * speed
        );

        RotateToward(direction);
    }

    private void MoveAwayFrom(Vector3 threatPosition, float speed)
    {
        Vector3 direction = transform.position - threatPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();

        rb.linearVelocity = new Vector3(
            direction.x * speed,
            rb.linearVelocity.y,
            direction.z * speed
        );

        RotateToward(direction);
    }

    private void CircleAroundTarget(float desiredDistance, float speed)
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.001f)
        {
            StopMoving();
            return;
        }

        float distance = toTarget.magnitude;
        Vector3 directionToTarget = toTarget.normalized;

        Vector3 tangentDirection = Vector3.Cross(Vector3.up, directionToTarget) * circleDirection;

        float distanceError = distance - desiredDistance;
        Vector3 distanceCorrection = directionToTarget * distanceError * circleDistanceCorrection;

        Vector3 moveDirection = tangentDirection + distanceCorrection;
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <= 0.001f)
        {
            StopMoving();
            return;
        }

        moveDirection.Normalize();

        rb.linearVelocity = new Vector3(
            moveDirection.x * speed,
            rb.linearVelocity.y,
            moveDirection.z * speed
        );

        FaceTarget(target.position);
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();
        RotateToward(direction);
    }

    private void RotateToward(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Quaternion nextRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    private void PickNewPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;

        patrolTargetPosition = startPosition + new Vector3(
            randomCircle.x,
            0f,
            randomCircle.y
        );

        waitTimer = patrolWaitDuration;
    }

    private void ChangeState(EnemyState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        isGuardMoving = false;

        if (currentState == EnemyState.AlertStare)
        {
            alertTimer = Random.Range(alertStareMinDuration, alertStareMaxDuration);
        }

        if (currentState == EnemyState.CloseFaceOff)
        {
            closeDecisionTimer = closeDecisionInterval;
        }

        if (currentState == EnemyState.ApproachAttack)
        {
            approachAttackTimer = approachAttackTimeout;
        }

        if (currentState == EnemyState.Backstep)
        {
            backstepTimer = backstepDuration;
            backstepCooldownTimer = backstepCooldown;
        }

        if (currentState == EnemyState.Guard)
        {
            guardTimer = guardDuration;
            guardCooldownTimer = guardCooldown;
        }

        if (currentState == EnemyState.AlertCircle)
        {
            circleTimer = Random.Range(alertCircleMinDuration, alertCircleMaxDuration);
            circleDirection = Random.value < 0.5f ? -1 : 1;
        }

        if (currentState == EnemyState.CloseCircle)
        {
            circleTimer = Random.Range(
                closeCircleMinDuration,
                closeCircleMaxDuration
            );

            circleDirection =
                Random.value < 0.5f ? -1 : 1;

            bool canGuardMove =
                useGuardedCloseCircle &&
                useGuardDefense &&
                guardCooldownTimer <= 0f;

            if (canGuardMove &&
                Random.value <= guardedCloseCircleChance)
            {
                isGuardMoving = true;
                guardCooldownTimer = guardCooldown;

                Debug.Log(
                    $"{gameObject.name} 방패를 들고 측면 이동"
                );
            }
            else
            {
                Debug.Log(
                    $"{gameObject.name} 일반 측면 이동"
                );
            }
        }

        Debug.Log($"{gameObject.name} 상태 변경: {currentState}");
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, alertMinDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, alertMaxDistance);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, closeFaceOffDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, loseDistance);
    }
}