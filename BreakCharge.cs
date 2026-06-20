using UnityEngine;
using System.Collections.Generic;

public class BreakCharge : MonoBehaviour
{
    private enum ChargeState
    {
        Ready,
        Charging,
        Recovery
    }

    [Header("References")]
    public PlayerController playerController;
    public Rigidbody rb;
    public CameraFollow cameraFollow;

    [Header("Charge Animation And Sword")]
    public Animator animator;
    public GameObject normalSword;
    public GameObject chargeSword;
    public GameObject blockSword;
    public string isChargingParameter = "IsCharging";

    [Header("Input")]
    public KeyCode chargeModifierKey = KeyCode.LeftShift;
    public int chargeMouseButton = 1; // 0 = 좌클릭, 1 = 우클릭

    [Header("Movement")]
    public float chargeSpeed = 18f;
    public float maxChargeDuration = 2.5f;
    public bool useMaxChargeDuration = false;
    public float recoveryDuration = 0.25f;

    [Header("Hit")]
    public LayerMask enemyLayer;
    public int chargeDamage = 40;
    public float knockbackMultiplier = 3.5f;
    public Vector3 hitBoxHalfExtents = new Vector3(0.7f, 1.0f, 0.9f);
    public float hitBoxForwardOffset = 0.8f;
    public float hitBoxHeightOffset = 1.0f;

    [Header("Hit Feedback")]
    public GameObject chargeHitEffectPrefab;
    public float hitShakeDuration = 0.12f;
    public float hitShakeStrength = 0.18f;
    public float tempEffectDuration = 0.2f;

    [Header("Direction")]
    public Transform cameraTransform;
    public float chargeTurnSpeed = 14f;

    [Header("Charge Impact")]
    public float hitStopDuration = 0.06f;
    public float hitStopSpeedMultiplier = 0.15f;
    public float heavyHitShakeDuration = 0.16f;
    public float heavyHitShakeStrength = 0.28f;

    [Header("Charge Interrupt Impact")]
    public float interruptShakeDuration = 0.18f;
    public float interruptShakeStrength = 0.35f;

    private float hitStopTimer;
    private int isChargingHash;
    private ChargeState chargeState = ChargeState.Ready;
    private Vector3 chargeDirection;
    private float timer;

    private readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            Debug.LogError("BreakCharge가 붙은 오브젝트에 Rigidbody가 없습니다.");
        }

        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
        isChargingHash = Animator.StringToHash(isChargingParameter);

        SetChargeVisual(false);
    }

    private void Update()
    {
        if (chargeState == ChargeState.Ready)
        {
            UpdateReadySwordVisual();
        }

        if (chargeState != ChargeState.Ready)
        {
            return;
        }

        if (IsChargeInputHeld())
        {
            TryStartCharge();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        switch (chargeState)
        {
            case ChargeState.Charging:
                UpdateCharging();
                break;

            case ChargeState.Recovery:
                UpdateRecovery();
                break;
        }
    }

    private bool IsChargeInputHeld()
    {
        return Input.GetKey(chargeModifierKey) && Input.GetMouseButton(chargeMouseButton);
    }

    private void TryStartCharge()
    {
        if (playerController != null && !playerController.CanStartExternalSkill())
        {
            return;
        }

        StartCharge();
    }

    private void StartCharge()
    {
        chargeState = ChargeState.Charging;
        timer = 0f;
        hitEnemies.Clear();

        SetChargeVisual(true);

        if (playerController != null)
        {
            playerController.SetExternalActionLock(true);
        }

        chargeDirection = GetDesiredChargeDirection();

        rb.angularVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.LookRotation(chargeDirection));

        Debug.Log("돌파 시작");
    }

    private void UpdateCharging()
    {
        timer += Time.fixedDeltaTime;

        rb.angularVelocity = Vector3.zero;

        Vector3 desiredDirection = GetDesiredChargeDirection();

        chargeDirection = Vector3.Slerp(
            chargeDirection,
            desiredDirection,
            chargeTurnSpeed * Time.fixedDeltaTime
        );

        if (chargeDirection == Vector3.zero)
        {
            chargeDirection = desiredDirection;
        }

        chargeDirection.Normalize();

        rb.MoveRotation(Quaternion.LookRotation(chargeDirection));

        float currentSpeed = chargeSpeed;

        if (hitStopTimer > 0f)
        {
            hitStopTimer -= Time.fixedDeltaTime;
            currentSpeed *= hitStopSpeedMultiplier;
        }

        Vector3 chargeVelocity = chargeDirection * currentSpeed;

        rb.linearVelocity = new Vector3(
            chargeVelocity.x,
            rb.linearVelocity.y,
            chargeVelocity.z
        );

        TryHitEnemy();

        if (!IsChargeInputHeld())
        {
            StartRecovery();
            return;
        }

        if (useMaxChargeDuration && timer >= maxChargeDuration)
        {
            StartRecovery();
        }
    }

    private void StartRecovery()
    {
        chargeState = ChargeState.Recovery;
        timer = recoveryDuration;

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        Debug.Log("돌파 리커버리");
    }

    private void UpdateRecovery()
    {
        timer -= Time.fixedDeltaTime;

        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        if (timer <= 0f)
        {
            EndCharge();
        }
    }

    private void EndCharge()
    {
        chargeState = ChargeState.Ready;
        SetChargeVisual(false);
        if (playerController != null)
        {
            playerController.SetExternalActionLock(false);
        }

        Debug.Log("돌파 종료");
    }

    private void TryHitEnemy()
    {
        Vector3 boxCenter =
            transform.position +
            Vector3.up * hitBoxHeightOffset +
            chargeDirection * hitBoxForwardOffset;

        Quaternion boxRotation = Quaternion.LookRotation(chargeDirection);

        Collider[] hitColliders = Physics.OverlapBox(
            boxCenter,
            hitBoxHalfExtents,
            boxRotation,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

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

            if (hitEnemies.Contains(enemyHealth))
            {
                continue;
            }

            hitEnemies.Add(enemyHealth);

            bool wasEnemyAttacking = IsEnemyAttacking(enemyHealth);

            if (enemyHitBox != null)
            {
                enemyHitBox.TakeHeavyHit(
                    chargeDamage,
                    transform.position,
                    knockbackMultiplier,
                    true
                );
            }
            else
            {
                enemyHealth.TakeHeavyDamage(
                    chargeDamage,
                    transform.position,
                    knockbackMultiplier,
                    true
                );
            }

            enemyHealth.ApplyDirectedKnockback(chargeDirection, knockbackMultiplier);

            if (wasEnemyAttacking)
            {
                Debug.Log($"돌파로 공격 중단! {enemyHealth.gameObject.name} 공격을 들이받아 끊음");
            }
            else
            {
                Debug.Log($"돌파 명중! 데미지: {chargeDamage}");
            }

            hitStopTimer = hitStopDuration;

            PlayChargeHitFeedback(hitCollider.transform.position, wasEnemyAttacking);
        }
    }

    private bool IsEnemyAttacking(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null)
        {
            return false;
        }

        EnemyAttack enemyAttack = enemyHealth.GetComponent<EnemyAttack>();

        if (enemyAttack == null)
        {
            return false;
        }

        return enemyAttack.IsAttacking;
    }

    private void PlayChargeHitFeedback(Vector3 hitPosition, bool isInterruptHit)
    {
        if (cameraFollow != null)
        {
            if (isInterruptHit)
            {
                cameraFollow.Shake(interruptShakeDuration, interruptShakeStrength);
            }
            else
            {
                cameraFollow.Shake(heavyHitShakeDuration, heavyHitShakeStrength);
            }
        }

        if (chargeHitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                chargeHitEffectPrefab,
                hitPosition + Vector3.up * 1f,
                Quaternion.identity
            );

            Destroy(effect, tempEffectDuration);
            return;
        }

        GameObject tempEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tempEffect.name = isInterruptHit ? "Temp_ChargeInterruptEffect" : "Temp_ChargeHitEffect";
        tempEffect.transform.position = hitPosition + Vector3.up * 1f;
        tempEffect.transform.localScale = isInterruptHit ? Vector3.one * 0.9f : Vector3.one * 0.6f;

        Collider effectCollider = tempEffect.GetComponent<Collider>();

        if (effectCollider != null)
        {
            Destroy(effectCollider);
        }

        Destroy(tempEffect, tempEffectDuration);
    }

    private Vector3 GetDesiredChargeDirection()
    {
        Vector3 inputDirection = GetCameraRelativeInputDirection();

        // 핵심:
        // WASD 입력이 있으면 그 방향으로 돌파한다.
        // W = 카메라 앞
        // S = 카메라 뒤
        // A/D = 카메라 기준 좌우
        if (inputDirection != Vector3.zero)
        {
            return inputDirection;
        }

        // WASD 입력이 없으면 기존처럼 카메라 정면으로 돌파한다.
        Vector3 direction;

        if (cameraTransform != null)
        {
            direction = cameraTransform.forward;
        }
        else
        {
            direction = transform.forward;
        }

        direction.y = 0f;

        if (direction == Vector3.zero)
        {
            direction = transform.forward;
            direction.y = 0f;
        }

        if (direction == Vector3.zero)
        {
            direction = Vector3.forward;
        }

        direction.Normalize();
        return direction;
    }

    private Vector3 GetCameraRelativeInputDirection()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.W)) z += 1f;

        Vector3 input = new Vector3(x, 0f, z);

        if (input == Vector3.zero)
        {
            return Vector3.zero;
        }

        input.Normalize();

        if (cameraTransform == null)
        {
            return input;
        }

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 direction =
            cameraForward * input.z +
            cameraRight * input.x;

        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        return direction;
    }

    private void OnDrawGizmos()
    {
        Vector3 direction = chargeDirection;

        if (direction == Vector3.zero)
        {
            direction = transform.forward;
            direction.y = 0f;

            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
        }

        Vector3 boxCenter =
            transform.position +
            Vector3.up * hitBoxHeightOffset +
            direction * hitBoxForwardOffset;

        Quaternion boxRotation = Quaternion.LookRotation(direction);

        Gizmos.color = chargeState == ChargeState.Charging ? Color.cyan : Color.blue;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            boxCenter,
            boxRotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, hitBoxHalfExtents * 2f);

        Gizmos.matrix = oldMatrix;
    }

    private void SetChargeVisual(bool isCharging)
    {
        if (normalSword != null)
        {
            normalSword.SetActive(!isCharging);
        }

        if (blockSword != null)
        {
            blockSword.SetActive(false);
        }

        if (chargeSword != null)
        {
            chargeSword.SetActive(isCharging);
        }

        if (animator != null)
        {
            animator.SetBool(
                isChargingHash,
                isCharging
            );
        }
    }

    private void UpdateReadySwordVisual()
    {
        bool isBlocking =
            playerController != null &&
            playerController.IsBlocking;

        if (normalSword != null)
        {
            normalSword.SetActive(!isBlocking);
        }

        if (blockSword != null)
        {
            blockSword.SetActive(isBlocking);
        }

        if (chargeSword != null)
        {
            chargeSword.SetActive(false);
        }
    }
}