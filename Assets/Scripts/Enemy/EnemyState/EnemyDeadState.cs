using UnityEngine;

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy enemy, EnemyAnimator enemyAnim) : base(enemy, enemyAnim) {}

    public override void EnterState()
    {
        Debug.Log("Enemy is Dead");
        EffectManager.Instance.PlayDeathEffect(enemy.transform.position);
        EmitManager.Instance.isEmittable = true;
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        // 적이 죽은 이후 실행시킬 로직

    }

    public override void CheckState()
    {

    }
}
