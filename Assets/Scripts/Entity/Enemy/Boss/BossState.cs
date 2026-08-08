public abstract class BossState : StateBase<BossStateMachine>
{
    /// <summary>
    /// 책임 및 기능: 각 State에서 Enemy가 움직이는 방식 (Enemy 전용 베이스)
    /// </summary>

    protected BossStateMachine stateMachine;
    protected Boss1 boss;

    public BossState(BossStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        this.boss = stateMachine.boss;
    }

    public override abstract void EnterState();
    public override abstract void UpdateState();
    public override abstract void ExitState();
    public override abstract void Move();
}
