using System;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour, IGrabbableContext, IKnockbackableContext
{
    /// <summary>
    /// 책임 및 기능: Enemy State가 관리되는 규칙
    /// </summary>
    public Enemy enemy;

    public IGrabbable GrabbableEntity => enemy;
    public IKnockbackable KnockbackableEntity => enemy;

    public StateBase<EnemyStateMachine> currentState;

    public StateBase<EnemyStateMachine> IdleState;
    public StateBase<EnemyStateMachine> AttackState;
    public StateBase<EnemyStateMachine> ChaseState;
    public StateBase<EnemyStateMachine> DeadState;
    public StateBase<EnemyStateMachine> KnockbackState;
    public StateBase<EnemyStateMachine> GrabbedState;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        IdleState = new EnemyIdleState(this);
        AttackState = new EnemyAttackState(this);
        ChaseState = new EnemyChaseState(this);
        DeadState = new EnemyDeadState(this);
        KnockbackState = new SharedKnockbackState<EnemyStateMachine>(this);
        GrabbedState = new SharedGrabbedState<EnemyStateMachine>(this);
    }

    public bool IsCurrentState(object state)
    {
        return currentState == state;
    }

    public void ChangeState(StateBase<EnemyStateMachine> nextState, bool force = false)
    {
        if (nextState == null)
        {
            throw new ArgumentNullException(nameof(nextState));
        }

        if (!force && (currentState == nextState || !enemy.canChangeState))
        {
            return;
        }

        currentState?.ExitState();
        currentState = nextState;
        //enemy.ResetPowerOutsideHitReaction();
        currentState.EnterState();
        enemy.NotifyStateEntered(currentState);
    }

    public void SetState(StateBase<EnemyStateMachine> nextState)
    {
        currentState = nextState ?? throw new ArgumentNullException(nameof(nextState));
    }

    public void UpdateState()
    {
        if (currentState == DeadState) return;

        currentState?.UpdateState();
    }

    public void Move()
    {
        currentState?.Move();
    }

    public void ChangeToDefaultState()
    {
        ChangeState(IdleState);
    }
}
