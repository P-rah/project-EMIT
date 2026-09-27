using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Strategy_UpAttack", menuName = "Boss Strategy/Up Attack")]
public class Boss1UpAttack: Boss1AttackStrategy
{
    public override IEnumerator ExecuteAttackRoutine(EnemyBase enemy, System.Action<bool> setCancellable)
    {
        setCancellable(false);

        enemy.NotifyAttackWindupStarted();

        yield return new WaitForSeconds(startUpTime);

        enemy.NotifyAttackStrikeStarted();

        yield return new WaitForSeconds(recoveryTime);
    }
}
