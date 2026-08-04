using System;

public sealed class BossStateMachine
{
    /// <summary>
    /// 책임 및 기능: Boss1 State가 관리되는 규칙
    /// </summary>

    public Boss1 Owner { get; }

    public BossState CurrentState { get; private set; }

    public BossMoveState MoveState { get; }
    public BossAttackState AttackState { get; }
    public BossRecoilState RecoilState { get; }
    public BossGroggyState GroggyState { get; }

    public BossStateMachine(Boss1 owner)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));

        MoveState = new BossMoveState(this);
        AttackState = new BossAttackState(this);
        RecoilState = new BossRecoilState(this);
        GroggyState = new BossGroggyState(this);
    }

    public bool IsCurrentState(BossState state)
    {
        return CurrentState == state;
    }

    public void ChangeState(BossState nextState, bool force = false)
    {
        if (nextState == null)
        {
            throw new ArgumentNullException(nameof(nextState));
        }

        if (!force && CurrentState == nextState)
        {
            return;
        }

        CurrentState?.ExitState();
        CurrentState = nextState;
        CurrentState.EnterState();
    }

    public void SetState(BossState nextState)
    {
        CurrentState = nextState
            ?? throw new ArgumentNullException(nameof(nextState));
    }

    public void UpdateState()
    {
        CurrentState?.UpdateState();
    }

    public void Move()
    {
        CurrentState?.Move();
    }
}
