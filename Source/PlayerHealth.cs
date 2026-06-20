using UnityEngine;
using System.Collections;

public enum PlayerDamageResult
{
    Damaged,
    Dodged,
    Blocked,
    Cooldown,
    Invalid
}

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHp = 100;
    private int currentHp;

    [Header("Damage Cooldown")]
    [Tooltip("피격 후 짧은 무적 시간. 연속 다단히트로 HP가 순식간에 빠지는 것을 방지한다.")]
    public float damageCooldown = 0.35f;

    [Header("References")]
    public PlayerController playerController;
    public CameraFollow cameraFollow;
    public Rigidbody rb;
    public CorpseShield corpseShield;

    [Header("Hit Feedback")]
    public float hitShakeDuration = 0.16f;
    public float hitShakeStrength = 0.28f;
    public float hitKnockbackForce = 7.5f;
    public float hitStunDuration = 0.18f;

    private Coroutine hitStunCoroutine;


    [Header("Block Feedback")]
    public float blockShakeDuration = 0.08f;
    public float blockShakeStrength = 0.12f;

    private float damageCooldownTimer;
    private bool isDead;

    public int CurrentHp
    {
        get { return currentHp; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    public int BlockSuccessSequence { get; private set; }
    public int HitReactionSequence { get; private set; }

    private void Awake()
    {
        currentHp = maxHp;

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (corpseShield == null)
        {
            corpseShield = GetComponent<CorpseShield>();
        }

        Debug.Log($"PlayerHealth 초기화 완료. HP: {currentHp}/{maxHp}");
    }

    private void Update()
    {
        if (damageCooldownTimer > 0f)
        {
            damageCooldownTimer -= Time.deltaTime;
        }
    }

    public PlayerDamageResult TakeDamage(int damage)
    {
        Vector3 fallbackAttackerPosition = transform.position - transform.forward;
        return TakeDamage(damage, fallbackAttackerPosition);
    }

    public PlayerDamageResult TakeDamage(int damage, Vector3 attackerPosition)
    {
        if (isDead)
        {
            return PlayerDamageResult.Invalid;
        }

        if (damage <= 0)
        {
            return PlayerDamageResult.Invalid;
        }

        if (damageCooldownTimer > 0f)
        {
            Debug.Log("Player 피격 쿨다운 중 - 데미지 무시");
            return PlayerDamageResult.Cooldown;
        }

        if (playerController != null && playerController.IsInvincible)
        {
            Debug.Log("구르기 무적 중 - 데미지 무시");
            return PlayerDamageResult.Dodged;
        }

        if (corpseShield != null && corpseShield.TryBlockAttack())
        {
            damageCooldownTimer = damageCooldown * 0.5f;

            if (cameraFollow != null)
            {
                cameraFollow.Shake(
                    blockShakeDuration,
                    blockShakeStrength
                );
            }

            Debug.Log("시신 방패로 공격 차단 - Player 데미지 무시");

            return PlayerDamageResult.Blocked;
        }

        if (playerController != null && playerController.IsBlocking)
        {
            damageCooldownTimer = damageCooldown * 0.5f;

            // 먼저 BlockHit 애니메이션 시작 신호를 보낸다.
            BlockSuccessSequence++;

            // 카메라 흔들림은 아주 조금 늦춰서
            // 신체 반응과 타이밍을 맞춘다.
            StartCoroutine(DelayedBlockShakeRoutine());

            Debug.Log("막기 성공 - 데미지 무시");
            return PlayerDamageResult.Blocked;
        }

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        damageCooldownTimer = damageCooldown;

        // 실제 HP가 감소했을 때만 피격 애니메이션 신호를 보낸다.
        HitReactionSequence++;

        PlayHitFeedback(attackerPosition);

        Debug.Log($"Player 피격! 데미지: {damage}, 현재 HP: {currentHp}/{maxHp}");

        if (currentHp <= 0)
        {
            Die();
        }

        return PlayerDamageResult.Damaged;
    }

    private void PlayHitFeedback(Vector3 attackerPosition)
    {
        // 피격 애니메이션이 먼저 시작된 뒤
        // 카메라가 아주 조금 늦게 흔들리도록 한다.
        StartCoroutine(DelayedHitShakeRoutine());

        ApplyHitKnockback(attackerPosition);

        if (playerController != null)
        {
            if (hitStunCoroutine != null)
            {
                StopCoroutine(hitStunCoroutine);
            }

            hitStunCoroutine = StartCoroutine(HitStunRoutine());
        }
    }

    private void ApplyHitKnockback(Vector3 attackerPosition)
    {
        if (rb == null)
        {
            return;
        }

        Vector3 knockbackDirection = transform.position - attackerPosition;
        knockbackDirection.y = 0f;

        if (knockbackDirection.sqrMagnitude <= 0.001f)
        {
            knockbackDirection = -transform.forward;
        }

        knockbackDirection.Normalize();

        rb.linearVelocity = new Vector3(
            knockbackDirection.x * hitKnockbackForce,
            rb.linearVelocity.y,
            knockbackDirection.z * hitKnockbackForce
        );
    }

    private IEnumerator HitStunRoutine()
    {
        playerController.SetExternalActionLock(true);

        yield return new WaitForSeconds(hitStunDuration);

        playerController.SetExternalActionLock(false);
        hitStunCoroutine = null;
    }

    public void Heal(int healAmount)
    {
        if (isDead)
        {
            return;
        }

        if (healAmount <= 0)
        {
            return;
        }

        currentHp += healAmount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"Player 회복. 현재 HP: {currentHp}/{maxHp}");
    }

    public void FullHeal()
    {
        if (isDead)
        {
            return;
        }

        currentHp = maxHp;

        Debug.Log($"Player 완전 회복. 현재 HP: {currentHp}/{maxHp}");
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player 사망");

        // 테스트 단계에서는 Player를 Destroy하지 않는다.
        // 나중에 사망 연출, 리트라이, 체크포인트를 붙일 때 여기서 처리한다.
    }
    private IEnumerator DelayedHitShakeRoutine()
    {
        yield return new WaitForSeconds(0.03f);

        if (cameraFollow != null)
        {
            cameraFollow.Shake(
                hitShakeDuration,
                hitShakeStrength
            );
        }
    }

    private IEnumerator DelayedBlockShakeRoutine()
    {
        yield return new WaitForSeconds(0.03f);

        if (cameraFollow != null)
        {
            cameraFollow.Shake(
                blockShakeDuration,
                blockShakeStrength
            );
        }
    }
}