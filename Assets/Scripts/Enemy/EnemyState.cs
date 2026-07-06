public abstract class EnemyState
{
    protected Enemy enemy;

    public EnemyState(Enemy enemy)
    {
        this.enemy = enemy;
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
            enemy.ChangeState(enemy.AttackState);
            return;
        }
        if (enemy.isPlayerInVision())
        {
            enemy.ChangeState(enemy.ChaseState);
            return;
        }
        enemy.ChangeState(enemy.IdleState);
    }

    public abstract void ExitState();

    public virtual void Move() {}
}
