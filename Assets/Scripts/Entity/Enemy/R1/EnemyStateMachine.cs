using System;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour, IGrabbableContext, IKnockbackableContext, IStateMachine
{
    /// <summary>
    /// 책임 및 기능: Enemy State가 관리되는 규칙
    /// </summary>
    public EnemyBase enemyBase { get; private set; }

    [SerializeField] private DeathEffectType deathEffectType;

    public IGrabbable GrabbableEntity => enemyBase;
    public IKnockbackable KnockbackableEntity => enemyBase;

    public StateBase<EnemyStateMachine> currentState;

    public StateBase<EnemyStateMachine> IdleState;
    public StateBase<EnemyStateMachine> AttackState;
    public StateBase<EnemyStateMachine> ChaseState;
    public StateBase<EnemyStateMachine> DeadState;
    public StateBase<EnemyStateMachine> KnockbackState;
    public StateBase<EnemyStateMachine> GrabbedState;

    private void Awake()
    {
        if (enemyBase == null)
        {
            enemyBase = GetComponent<EnemyBase>();
        }

        IdleState = new EnemyIdleState(this);
        AttackState = new EnemyAttackState(this);
        ChaseState = new EnemyChaseState(this);
        DeadState = new EnemyDeadState(this, deathEffectType);
        KnockbackState = new SharedKnockbackState<EnemyStateMachine>(this, enemyBase);
        GrabbedState = new SharedGrabbedState<EnemyStateMachine>(this, enemyBase);
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

        if (!force && (currentState == nextState || !enemyBase.canChangeState))
        {
            return;
        }

        currentState?.ExitState();
        currentState = nextState;

        currentState.EnterState();
        enemyBase.NotifyStateEntered(currentState);
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
