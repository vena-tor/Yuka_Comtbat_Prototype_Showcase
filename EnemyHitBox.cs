using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    [Header("Target")]
    public EnemyHealth enemyHealth;

    [Header("Damage")]
    public float damageMultiplier = 1f;

    private void Awake()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth == null)
        {
            Debug.LogError($"{gameObject.name}의 EnemyHitBox가 EnemyHealth를 찾지 못했습니다.");
        }
    }

    public EnemyHealth GetEnemyHealth()
    {
        return enemyHealth;
    }

    public void TakeHit(int damage, Vector3 attackerPosition)
    {
        TakeHit(damage, attackerPosition, 1f);
    }

    public void TakeHit(
     int damage,
     Vector3 attackerPosition,
     float knockbackMultiplier)
    {
        TakeHit(
            damage,
            attackerPosition,
            knockbackMultiplier,
            false
        );
    }

    public void TakeHit(
        int damage,
        Vector3 attackerPosition,
        float knockbackMultiplier,
        bool ignoreGuard)
    {
        if (enemyHealth == null)
        {
            return;
        }

        int finalDamage = Mathf.RoundToInt(
            damage * damageMultiplier
        );

        enemyHealth.TakeDamage(
            finalDamage,
            attackerPosition,
            knockbackMultiplier,
            ignoreGuard
        );
    }
    public void TakeHeavyHit(
    int damage,
    Vector3 attackerPosition,
    float knockbackMultiplier,
    bool ignoreGuard)
    {
        if (enemyHealth == null)
        {
            return;
        }

        int finalDamage = Mathf.RoundToInt(
            damage * damageMultiplier
        );

        enemyHealth.TakeHeavyDamage(
            finalDamage,
            attackerPosition,
            knockbackMultiplier,
            ignoreGuard
        );
    }
}