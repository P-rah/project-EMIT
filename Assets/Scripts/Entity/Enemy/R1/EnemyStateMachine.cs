using System;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour, IGrabbableContext, IKnockbackableContext, IEnemyStateMachine
{
    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private DeathEffectType deathEffectType;

    public EnemyBase EnemyBase => enemyBase;

    public IGrabbable GrabbableEntity => enemyBase;
    public IKnockbackable KnockbackableEntity => enemyBase;

    public IState CurrentState { get; private set; }

    public IState IdleState { get; private set; }
    public IState AttackState { get; private set; }
    public IState ChaseState { get; private set; }
    public IState DeadState { get; private set; }
    public IState KnockbackState { get; private set; }
    public IState GrabbedState { get; private set; }

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

        KnockbackState = new SharedKnockbackState(this);
        GrabbedState = new SharedGrabbedState(this);
    }

    public bool IsCurrentState(object state)
    {
        return CurrentState == state;
    }

    public void ChangeState(IState nextState, bool force = false)
    {
        if (nextState == null)
        {
            throw new ArgumentNullException(nameof(nextState));
        }

        if (!force && (CurrentState == nextState || !enemyBase.canChangeState))
        {
            return;
        }

        CurrentState?.ExitState();
        CurrentState = nextState;

        CurrentState.EnterState();
        enemyBase.NotifyStateEntered(CurrentState);
    }

    public void SetState(IState nextState)
    {
        CurrentState = nextState ?? throw new ArgumentNullException(nameof(nextState));
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

    public void ChangeToDefaultState()
    {
        ChangeState(IdleState);
    }
}