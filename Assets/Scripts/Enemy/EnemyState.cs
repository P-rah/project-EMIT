public abstract class EnemyState
{
    protected Enemy enemy;
    protected EnemyAnimator enemyAnim;

    public EnemyState(Enemy enemy, EnemyAnimator enemyAnim)
    {
        this.enemy = enemy;
        this.enemyAnim = enemyAnim;
    }

    public abstract void EnterState();

    public virtual void UpdateState()
    {
        CheckState();
    }

    public virtual void CheckState()
    {
        if (enemy.isPlayerInAttackRange())
        {
            enemy.ChangeState(enemy.AttackState, false);
            return;
        }
        if (enemy.isPlayerInVision())
        {
            enemy.ChangeState(enemy.ChaseState, false);
            return;
        }
        enemy.ChangeState(enemy.IdleState, false);
    }

    public abstract void ExitState();

    public virtual void Move() {}
}
