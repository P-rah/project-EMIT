using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyAnimator enemyAnim) : base(enemy, enemyAnim) { }

    public override void EnterState()
    {
        enemyAnim.PlayIdle();
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
    }

    // public override void CheckState() { }

}
