using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Strategy_DownAttack", menuName = "Boss Strategy/Down Attack")]
public class Boss1DownAttack : Boss1AttackStrategy
{
    [Header("연속 공격 카운트")]
    [SerializeField] private int attackCount = 3;

    public override IEnumerator ExecuteAttackRoutine(EnemyBase enemy, System.Action<bool> setCancellable)
    {
        for(int i = 0; i < attackCount; i++)
        {
            setCancellable(false);

            enemy.NotifyAttackWindupStarted();

            yield return new WaitForSeconds(startUpTime);

            enemy.NotifyAttackStrikeStarted();

            yield return new WaitForSeconds(recoveryTime);
            enemy.Flip();
        }
    }
}