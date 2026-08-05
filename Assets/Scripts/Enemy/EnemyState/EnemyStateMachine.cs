using System;

public sealed class EnemyStateMachine
{
    /// <summary>
    /// 책임 및 기능: Enemy State가 관리되는 규칙
    /// </summary>

    public Enemy Owner { get; }

    public EnemyState CurrentState { get; private set; }

    public EnemyIdleState IdleState { get; }
    public EnemyAttackState AttackState { get; }
    public EnemyChaseState ChaseState { get; }
    public EnemyDeadState DeadState { get; }
    public EnemyKnockbackState KnockbackState { get; }  //넉백(밀치기) 상태
    public EnemyGrabbedState GrabbedState { get; }     //넉백이랑 당겨오는 상태랑 분리

    public EnemyStateMachine(Enemy owner)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));

        IdleState = new EnemyIdleState(this);
        AttackState = new EnemyAttackState(this);
        ChaseState = new EnemyChaseState(this);
        DeadState = new EnemyDeadState(this);
        KnockbackState = new EnemyKnockbackState(this);
        GrabbedState = new EnemyGrabbedState(this);
    }

    public bool IsCurrentState(EnemyState state)
    {
        return CurrentState == state;
    }

    public void ChangeState(EnemyState nextState, bool force = false)
    {
        if (nextState == null)
        {
            throw new ArgumentNullException(nameof(nextState));
        }

        if (!force && (CurrentState == nextState || !Owner.canChangeState))
        {
            return;
        }

        CurrentState?.ExitState();
        CurrentState = nextState;
        Owner.ResetPowerOutsideHitReaction();
        CurrentState.EnterState();
        Owner.NotifyStateEntered(CurrentState);
    }

    public void SetState(EnemyState nextState)
    {
        CurrentState = nextState
            ?? throw new ArgumentNullException(nameof(nextState));
    }

    public void UpdateState()
    {
        if (CurrentState == DeadState) return;

        CurrentState?.UpdateState();
    }

    public void Move()
    {
        CurrentState?.Move();
    }
}
