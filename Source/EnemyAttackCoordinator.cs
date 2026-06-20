using UnityEngine;
using System.Collections.Generic;

public class EnemyAttackCoordinator : MonoBehaviour
{
    [Header("Attack Slot")]
    public int maxSimultaneousAttackers = 1;

    private readonly HashSet<EnemyAttack> currentAttackers = new HashSet<EnemyAttack>();

    public bool CanRequestAttack(EnemyAttack enemyAttack)
    {
        if (enemyAttack == null)
        {
            return false;
        }

        if (currentAttackers.Contains(enemyAttack))
        {
            return true;
        }

        return currentAttackers.Count < maxSimultaneousAttackers;
    }

    public bool RequestAttackSlot(EnemyAttack enemyAttack)
    {
        if (enemyAttack == null)
        {
            return false;
        }

        if (currentAttackers.Contains(enemyAttack))
        {
            return true;
        }

        if (currentAttackers.Count >= maxSimultaneousAttackers)
        {
            return false;
        }

        currentAttackers.Add(enemyAttack);

        Debug.Log($"{enemyAttack.gameObject.name} °ø°Ý±Ç È¹µæ");

        return true;
    }

    public void ReleaseAttackSlot(EnemyAttack enemyAttack)
    {
        if (enemyAttack == null)
        {
            return;
        }

        if (currentAttackers.Remove(enemyAttack))
        {
            Debug.Log($"{enemyAttack.gameObject.name} °ø°Ý±Ç ¹Ý³³");
        }
    }
}