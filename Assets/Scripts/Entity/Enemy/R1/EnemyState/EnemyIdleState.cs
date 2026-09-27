public class EnemyIdleState: EnemyState
{
    public EnemyIdleState(IEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ExitState()
    {
    }

    public override void Move()
    {
    }
}
