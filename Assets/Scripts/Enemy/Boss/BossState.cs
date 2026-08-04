public abstract class BossState
{
    /// <summary>
    /// 책임 및 기능: 각 State에서 Boss1이 동작하는 방식
    /// </summary>
    protected BossStateMachine stateMachine;
    protected Boss1 boss;

    public BossState(BossStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.boss = stateMachine.Owner;
    }

    public abstract void EnterState();

    public virtual void UpdateState() { }

    public abstract void ExitState();

    public virtual void Move() { }
}
