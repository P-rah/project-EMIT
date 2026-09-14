public abstract class EnemyState : StateBase<EnemyStateMachine>
{
    /// <summary>
    /// 책임 및 기능: 각 State에서 Enemy가 움직이는 방식 (Enemy 전용 베이스)
    /// </summary>

    protected EnemyStateMachine stateMachine;
    protected EnemyBase enemy;

    public EnemyState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        this.enemy = stateMachine.GetComponent<EnemyBase>();
    }

    public override abstract void EnterState();
    public override abstract void ExitState();

    public override void UpdateState()
    {
        CheckState();
    }

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
        if (stateMachine.currentState != stateMachine.IdleState)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
    }
}
