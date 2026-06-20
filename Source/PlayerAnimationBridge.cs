using UnityEngine;

public class PlayerAnimationBridge : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody playerRigidbody;
    public PlayerController playerController;
    public PlayerHealth playerHealth;

    [Header("Animator Parameters")]
    public string moveSpeedParameter = "MoveSpeed";
    public string moveXParameter = "MoveX";
    public string moveYParameter = "MoveY";
    public string isSprintingParameter = "IsSprinting";
    public string isCorpseRunningParameter = "IsCorpseRunning";
    public string isHoldingCorpseParameter = "IsHoldingCorpse";
    public string isBlockingParameter = "IsBlocking";
    public string blockHitTriggerParameter = "BlockHit";
    public string hitTriggerParameter = "Hit";
    public string dodgeTriggerParameter = "Dodge";
    public string attack1TriggerParameter = "Attack1";
    public string attack2TriggerParameter = "Attack2";
    public string attack3TriggerParameter = "Attack3";

    [Header("Smoothing")]
    public float parameterDampTime = 0.08f;

    private int moveSpeedHash;
    private int moveXHash;
    private int moveYHash;
    private int isSprintingHash;
    private int isCorpseRunningHash;
    private int isHoldingCorpseHash;
    private int isBlockingHash;
    private int blockHitTriggerHash;
    private int lastBlockSuccessSequence;

    private int hitTriggerHash;
    private int lastHitReactionSequence;
    private int dodgeTriggerHash;
    private int lastDodgeStartSequence;

    private int attack1TriggerHash;
    private int attack2TriggerHash;
    private int attack3TriggerHash;

    private int lastAttackStartSequence;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponentInParent<Rigidbody>();
        }

        if (playerController == null)
        {
            playerController =
                GetComponentInParent<PlayerController>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                GetComponentInParent<PlayerHealth>();
        }

        moveSpeedHash =
            Animator.StringToHash(moveSpeedParameter);

        moveXHash =
            Animator.StringToHash(moveXParameter);

        moveYHash =
            Animator.StringToHash(moveYParameter);

        isSprintingHash =
            Animator.StringToHash(isSprintingParameter);

        isCorpseRunningHash =
            Animator.StringToHash(isCorpseRunningParameter);

        isHoldingCorpseHash =
            Animator.StringToHash(isHoldingCorpseParameter);

        isBlockingHash =
            Animator.StringToHash(isBlockingParameter);

        blockHitTriggerHash =
            Animator.StringToHash(blockHitTriggerParameter);

        hitTriggerHash = 
            Animator.StringToHash(hitTriggerParameter);
        
        dodgeTriggerHash =
            Animator.StringToHash(dodgeTriggerParameter);

        if (playerHealth != null)
        {
            lastBlockSuccessSequence =
                playerHealth.BlockSuccessSequence;

            lastHitReactionSequence =
                playerHealth.HitReactionSequence;
        }

        attack1TriggerHash =
        Animator.StringToHash(attack1TriggerParameter);

        attack2TriggerHash =
            Animator.StringToHash(attack2TriggerParameter);

        attack3TriggerHash =
        attack3TriggerHash =
            Animator.StringToHash(attack3TriggerParameter);

        if (playerController != null)
        {
            lastAttackStartSequence =
                playerController.AttackStartSequence;

            lastDodgeStartSequence =
                playerController.DodgeStartSequence;
        }

        if (animator == null)
        {
            Debug.LogError(
                "PlayerAnimationBridge가 Animator를 찾지 못했습니다."
            );
        }

        if (playerRigidbody == null)
        {
            Debug.LogError(
                "PlayerAnimationBridge가 Player Rigidbody를 찾지 못했습니다."
            );
        }

        if (playerController == null)
        {
            Debug.LogError(
                "PlayerAnimationBridge가 PlayerController를 찾지 못했습니다."
            );
        }
    }

    private void Update()
    {
        if (animator == null ||
            playerRigidbody == null)
        {
            return;
        }

        Vector3 horizontalVelocity =
            playerRigidbody.linearVelocity;

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
                playerRigidbody.transform
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

        bool isSprinting =
            playerController != null &&
            playerController.IsSprinting;

        bool isCorpseRunning =
            playerController != null &&
            playerController.IsCorpseRunning;

        animator.SetBool(
            isSprintingHash,
            isSprinting
        );

        animator.SetBool(
            isCorpseRunningHash,
            isCorpseRunning
        );

        bool isHoldingCorpse = playerController != null && playerController.corpseShield != null && playerController.corpseShield.IsHoldingCorpse;

        animator.SetBool(
            isHoldingCorpseHash,
            isHoldingCorpse
        );

        bool isBlocking =playerController != null && playerController.IsBlocking;

        animator.SetBool(
            isBlockingHash,
            isBlocking
        );

        UpdateAttackAnimation();
        UpdateBlockHitAnimation();
        UpdateHitAnimation();
        UpdateDodgeAnimation();
    }
    private void UpdateAttackAnimation()
    {
        if (animator == null || playerController == null)
        {
            return;
        }

        int currentSequence =
            playerController.AttackStartSequence;

        if (currentSequence == lastAttackStartSequence)
        {
            return;
        }

        lastAttackStartSequence = currentSequence;

        animator.ResetTrigger(attack1TriggerHash);
        animator.ResetTrigger(attack2TriggerHash);
        animator.ResetTrigger(attack3TriggerHash);

        switch (playerController.CurrentComboStep)
        {
            case 1:
                animator.SetTrigger(attack1TriggerHash);
                break;

            case 2:
                animator.SetTrigger(attack2TriggerHash);
                break;

            case 3:
                animator.SetTrigger(attack3TriggerHash);
                break;
        }
    }
    private void UpdateBlockHitAnimation()
    {
        if (animator == null || playerHealth == null)
        {
            return;
        }

        int currentSequence =
            playerHealth.BlockSuccessSequence;

        if (currentSequence == lastBlockSuccessSequence)
        {
            return;
        }

        lastBlockSuccessSequence = currentSequence;

        animator.ResetTrigger(blockHitTriggerHash);
        animator.SetTrigger(blockHitTriggerHash);
    }
    private void UpdateHitAnimation()
    {
        if (animator == null || playerHealth == null)
        {
            return;
        }

        int currentSequence =
            playerHealth.HitReactionSequence;

        if (currentSequence == lastHitReactionSequence)
        {
            return;
        }

        lastHitReactionSequence = currentSequence;

        animator.ResetTrigger(hitTriggerHash);
        animator.SetTrigger(hitTriggerHash);
    }
    private void UpdateDodgeAnimation()
    {
        if (animator == null || playerController == null)
        {
            return;
        }

        int currentSequence =
            playerController.DodgeStartSequence;

        if (currentSequence == lastDodgeStartSequence)
        {
            return;
        }

        lastDodgeStartSequence = currentSequence;

        animator.ResetTrigger(dodgeTriggerHash);
        animator.SetTrigger(dodgeTriggerHash);
    }
}