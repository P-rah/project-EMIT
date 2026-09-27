public abstract class EnemyState : IState
{
    protected IEnemyStateMachine stateMachine;
    protected EnemyBase enemy;

    public EnemyState(IEnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.enemy = stateMachine.EnemyBase;
    }

    public abstract void EnterState();
    public abstract void ExitState();

    public virtual void UpdateState()
    {
        CheckState();
    }

    public virtual void Move() { }

    public virtual void CheckState()
    {
        if (enemy.isPlayerInAttackRange())
        {
            stateMachine.ChangeState(stateMachine.AttackState);
            return;
        }
        if (enemy.isPlayerInVision())
        {
            stateMachine.ChangeState(stateMachine.ChaseState);
            return;
        }
        if (stateMachine.CurrentState != stateMachine.IdleState)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
    }
}
