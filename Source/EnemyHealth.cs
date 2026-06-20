using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    private EnemySimpleAI simpleAI;
    private EnemyAttack enemyAttack;

    [Header("Health")]
    public int maxHp = 100;
    private int currentHp;
    public int HitReactionSequence { get; private set; }
    public int HeavyHitReactionSequence { get; private set; }
    public int DeathReactionSequence { get; private set; }

    [Header("Heavy Hit Reaction")]
    public float heavyHitStunDuration = 0.65f;

    [Header("Death Animation")]
    public float deathAnimationDuration = 1.5f;
    public float deathRemainDuration = 0.3f;

    private bool isDead;
    public bool IsDead
    {
        get { return isDead; }
    }
    public int CurrentHp
    {
        get { return currentHp; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    [Header("Hit Reaction")]
    public float knockbackForce = 6f;
    public float hitStunDuration = 0.25f;

    private Rigidbody rb;
    private EnemyFleeAI fleeAI;

    private void Awake()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody>();
        fleeAI = GetComponent<EnemyFleeAI>();

        if (rb == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Rigidbody가 없습니다. 넉백이 작동하지 않을 수 있습니다.");
        }

        simpleAI = GetComponent<EnemySimpleAI>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(
            damage,
            transform.position - transform.forward,
            1f,
            false
        );
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        TakeDamage(
            damage,
            attackerPosition,
            1f,
            false
        );
    }

    public void TakeDamage(
        int damage,
        Vector3 attackerPosition,
        float knockbackMultiplier)
    {
        TakeDamage(
            damage,
            attackerPosition,
            knockbackMultiplier,
            false
        );
    }

    public void TakeDamage(
    int damage,
    Vector3 attackerPosition,
    float knockbackMultiplier,
    bool ignoreGuard)
    {
        TakeDamageInternal(
            damage,
            attackerPosition,
            knockbackMultiplier,
            ignoreGuard,
            false
        );
    }

    public void TakeHeavyDamage(
        int damage,
        Vector3 attackerPosition,
        float knockbackMultiplier,
        bool ignoreGuard)
    {
        TakeDamageInternal(
            damage,
            attackerPosition,
            knockbackMultiplier,
            ignoreGuard,
            true
        );
    }

    private void TakeDamageInternal(
        int damage,
        Vector3 attackerPosition,
        float knockbackMultiplier,
        bool ignoreGuard,
        bool useHeavyHitReaction)
    {
        if (isDead)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        if (simpleAI != null && simpleAI.IsGuarding)
        {
            if (ignoreGuard)
            {
                // 돌파는 피해 감소를 무시하고 방어를 깨뜨린다.
                simpleAI.ResolveGuardHit(true);
            }
            else
            {
                damage = Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        damage *
                        simpleAI.GuardDamageMultiplier
                    )
                );

                simpleAI.ResolveGuardHit(false);

                Debug.Log(
                    $"{gameObject.name} 방어 피해 감소! 실제 데미지: {damage}"
                );
            }
        }

        currentHp -= damage;

        // 사망하지 않은 경우에만 피격 애니메이션 신호를 보낸다.
        if (currentHp > 0)
        {
            if (useHeavyHitReaction)
            {
                HeavyHitReactionSequence++;
            }
            else
            {
                HitReactionSequence++;
            }
        }

        Debug.Log(
            $"{gameObject.name} 맞음! 현재 HP: {currentHp}"
        );

        ApplyKnockback(
            attackerPosition,
            knockbackMultiplier
        );

        float appliedHitStunDuration =
            useHeavyHitReaction
                ? heavyHitStunDuration
                : hitStunDuration;

        if (enemyAttack != null)
        {
            enemyAttack.InterruptAttack(
                appliedHitStunDuration
            );
        }

        if (simpleAI != null)
        {
            simpleAI.PauseAI(
                appliedHitStunDuration
            );
        }

        if (fleeAI != null)
        {
            fleeAI.PauseMovement(
                appliedHitStunDuration
            );
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector3 attackerPosition, float knockbackMultiplier)
    {
        if (rb == null || rb.isKinematic)
        {
            return;
        }

        Vector3 knockbackDirection = transform.position - attackerPosition;
        knockbackDirection.y = 0f;

        if (knockbackDirection == Vector3.zero)
        {
            knockbackDirection = -transform.forward;
        }

        knockbackDirection.Normalize();

        float finalKnockbackForce = knockbackForce * knockbackMultiplier;

        rb.linearVelocity = new Vector3(
            knockbackDirection.x * finalKnockbackForce,
            rb.linearVelocity.y,
            knockbackDirection.z * finalKnockbackForce
        );
    }

    public void ApplyDirectedKnockback(Vector3 direction, float knockbackMultiplier)
    {
        if (rb == null || rb.isKinematic)
        {
            return;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();

        float finalKnockbackForce = knockbackForce * knockbackMultiplier;

        rb.linearVelocity = new Vector3(
            direction.x * finalKnockbackForce,
            rb.linearVelocity.y,
            direction.z * finalKnockbackForce
        );
    }
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        DeathReactionSequence++;

        Debug.Log($"{gameObject.name} 사망");

        StartCoroutine(DeathAnimationRoutine());
    }

    private IEnumerator DeathAnimationRoutine()
    {
        // AI와 공격 기능부터 정지한다.
        if (simpleAI != null)
        {
            simpleAI.enabled = false;
        }

        if (enemyAttack != null)
        {
            enemyAttack.InterruptAttack(0f);
            enemyAttack.enabled = false;
        }

        if (fleeAI != null)
        {
            fleeAI.enabled = false;
        }

        // 기존 넉백이나 이동을 멈춘다.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 죽은 Enemy가 플레이어 이동과 공격 판정을 막지 않게 한다.
        Collider bodyCollider = GetComponent<Collider>();

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        // Death 애니메이션이 끝날 때까지 기다린다.
        yield return new WaitForSeconds(
            deathAnimationDuration
        );

        // 마지막 쓰러진 자세가 잠깐 보이게 한다.
        yield return new WaitForSeconds(
            deathRemainDuration
        );

        // 동일한 Enemy 오브젝트를 시신 방패용 오브젝트로 전환한다.
        CorpseObject corpseObject =
            GetComponent<CorpseObject>();

        if (corpseObject == null)
        {
            corpseObject =
                gameObject.AddComponent<CorpseObject>();
        }

        corpseObject.PrepareAsCorpse();

        Debug.Log(
            $"{gameObject.name} 시신으로 전환 완료"
        );
    }
}