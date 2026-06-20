using UnityEngine;

public class EnemyAnimationBridge : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody enemyRigidbody;
    public EnemyAttack enemyAttack;
    public EnemySimpleAI enemySimpleAI;
    public EnemyHealth enemyHealth;

    [Header("Movement Parameters")]
    public string moveSpeedParameter = "MoveSpeed";
    public string moveXParameter = "MoveX";
    public string moveYParameter = "MoveY";
    public string isSprintingParameter = "IsSprinting";

    [Header("Combat Parameters")]
    public string isBlockingParameter = "IsBlocking";
    public string attack1TriggerParameter = "Attack1";
    public string attack2TriggerParameter = "Attack2";
    public string hitTriggerParameter = "Hit";
    public string heavyHitTriggerParameter = "HeavyHit";
    public string deathTriggerParameter = "Death";

    [Header("Smoothing")]
    public float parameterDampTime = 0.08f;

    private int moveSpeedHash;
    private int moveXHash;
    private int moveYHash;
    private int isSprintingHash;

    private int isBlockingHash;
    private int attack1TriggerHash;
    private int attack2TriggerHash;

    private int hitTriggerHash;
    private int heavyHitTriggerHash;
    private int lastHeavyHitReactionSequence;
    private int lastHitReactionSequence;

    private int deathTriggerHash;
    private int lastDeathReactionSequence;

    private int lastAttackStartSequence;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponentInParent<Rigidbody>();
        }

        if (enemyAttack == null)
        {
            enemyAttack = GetComponentInParent<EnemyAttack>();
        }

        if (enemySimpleAI == null)
        {
            enemySimpleAI =
                GetComponentInParent<EnemySimpleAI>();
        }

        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponentInParent<EnemyHealth>();
        }

        moveSpeedHash =
            Animator.StringToHash(moveSpeedParameter);

        moveXHash =
            Animator.StringToHash(moveXParameter);

        moveYHash =
            Animator.StringToHash(moveYParameter);

        isSprintingHash =
            Animator.StringToHash(isSprintingParameter);

        isBlockingHash =
            Animator.StringToHash(isBlockingParameter);

        attack1TriggerHash =
            Animator.StringToHash(attack1TriggerParameter);

        attack2TriggerHash =
            Animator.StringToHash(attack2TriggerParameter);

        hitTriggerHash =   
            Animator.StringToHash(hitTriggerParameter);

        heavyHitTriggerHash =
            Animator.StringToHash(heavyHitTriggerParameter);

        deathTriggerHash = 
            Animator.StringToHash(deathTriggerParameter);

        if (enemyAttack != null)
        {
            lastAttackStartSequence =
                enemyAttack.AttackStartSequence;
        }

        if (animator == null)
        {
            Debug.LogError(
                "EnemyAnimationBridge가 Animator를 찾지 못했습니다."
            );
        }

        if (enemyRigidbody == null)
        {
            Debug.LogError(
                "EnemyAnimationBridge가 Enemy Rigidbody를 찾지 못했습니다."
            );
        }

        if (enemyAttack == null)
        {
            Debug.LogError(
                "EnemyAnimationBridge가 EnemyAttack을 찾지 못했습니다."
            );
        }

        if (enemySimpleAI == null)
        {
            Debug.LogError(
                "EnemyAnimationBridge가 EnemySimpleAI를 찾지 못했습니다."
            );
        }

        if (enemyHealth != null)
        {
            lastHitReactionSequence =
                enemyHealth.HitReactionSequence;

            lastHeavyHitReactionSequence =
                enemyHealth.HeavyHitReactionSequence;

            lastDeathReactionSequence =
                enemyHealth.DeathReactionSequence;
        }
    }

    private void Update()
    {
        if (animator == null || enemyRigidbody == null)
        {
            return;
        }

        if (enemyHealth != null && enemyHealth.IsDead)
        {
            UpdateDeathAnimation();
            return;
        }

        UpdateMovementAnimation();
        UpdateGuardAnimation();
        UpdateAttackAnimation();
        UpdateHitAnimation();
        UpdateHeavyHitAnimation();
    }

    private void UpdateMovementAnimation()
    {
        Vector3 horizontalVelocity =
            enemyRigidbody.linearVelocity;

        horizontalVelocity.y = 0f;

        float moveSpeed =
            horizontalVelocity.magnitude;

        Vector3 localMoveDirection =
            Vector3.zero;

        if (moveSpeed > 0.01f)
        {
            Vector3 worldMoveDirection =
                horizontalVelocity / moveSpeed;

            localMoveDirection =
                enemyRigidbody.transform
                    .InverseTransformDirection(
                        worldMoveDirection
                    );
        }

        animator.SetFloat(
            moveSpeedHash,
            moveSpeed,
            parameterDampTime,
            Time.deltaTime
        );

        animator.SetFloat(
            moveXHash,
            localMoveDirection.x,
            parameterDampTime,
            Time.deltaTime
        );

        animator.SetFloat(
            moveYHash,
            localMoveDirection.z,
            parameterDampTime,
            Time.deltaTime
        );

        animator.SetBool(
            isSprintingHash,
            false
        );
    }

    private void UpdateGuardAnimation()
    {
        bool isGuarding =
            enemySimpleAI != null &&
            enemySimpleAI.IsGuarding;

        animator.SetBool(
            isBlockingHash,
            isGuarding
        );
    }

    private void UpdateAttackAnimation()
    {
        if (enemyAttack == null)
        {
            return;
        }

        int currentSequence =
            enemyAttack.AttackStartSequence;

        if (currentSequence == lastAttackStartSequence)
        {
            return;
        }

        lastAttackStartSequence = currentSequence;

        animator.ResetTrigger(attack1TriggerHash);
        animator.ResetTrigger(attack2TriggerHash);

        switch (enemyAttack.CurrentAttackStep)
        {
            case 1:
                animator.SetTrigger(attack1TriggerHash);
                break;

            case 2:
                animator.SetTrigger(attack2TriggerHash);
                break;
        }
    }
    private void UpdateHitAnimation()
    {
        if (animator == null || enemyHealth == null)
        {
            return;
        }

        int currentSequence =
            enemyHealth.HitReactionSequence;

        if (currentSequence == lastHitReactionSequence)
        {
            return;
        }

        lastHitReactionSequence = currentSequence;

        // 공격 Trigger가 같은 순간 남아 있더라도
        // Hit이 공격 애니메이션을 확실히 끊게 한다.
        animator.ResetTrigger(attack1TriggerHash);
        animator.ResetTrigger(attack2TriggerHash);
        animator.ResetTrigger(hitTriggerHash);

        animator.SetTrigger(hitTriggerHash);
    }

    private void UpdateHeavyHitAnimation()
    {
        if (animator == null || enemyHealth == null)
        {
            return;
        }

        int currentSequence =
            enemyHealth.HeavyHitReactionSequence;

        if (currentSequence ==
            lastHeavyHitReactionSequence)
        {
            return;
        }

        lastHeavyHitReactionSequence = currentSequence;

        animator.ResetTrigger(attack1TriggerHash);
        animator.ResetTrigger(attack2TriggerHash);
        animator.ResetTrigger(hitTriggerHash);
        animator.ResetTrigger(heavyHitTriggerHash);

        animator.SetTrigger(heavyHitTriggerHash);
    }
    private void UpdateDeathAnimation()
    {
        if (animator == null || enemyHealth == null)
        {
            return;
        }

        int currentSequence =
            enemyHealth.DeathReactionSequence;

        if (currentSequence ==
            lastDeathReactionSequence)
        {
            return;
        }

        lastDeathReactionSequence = currentSequence;

        animator.ResetTrigger(attack1TriggerHash);
        animator.ResetTrigger(attack2TriggerHash);
        animator.ResetTrigger(hitTriggerHash);
        animator.ResetTrigger(heavyHitTriggerHash);
        animator.ResetTrigger(deathTriggerHash);

        animator.SetBool(
            isBlockingHash,
            false
        );

        animator.SetTrigger(
            deathTriggerHash
        );
    }
}