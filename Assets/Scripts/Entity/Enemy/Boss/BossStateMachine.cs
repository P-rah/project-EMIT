using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossStateMachine : MonoBehaviour, IGrabbableContext, IKnockbackableContext
{
    /// <summary>
    /// 책임 및 기능: Boss1 State가 관리되는 규칙
    /// </summary>

    public Boss1 boss;

    public IGrabbable GrabbableEntity => boss;
    public IKnockbackable KnockbackableEntity => boss;

    public StateBase<BossStateMachine> CurrentState;

    public StateBase<BossStateMachine> MoveState;
    public StateBase<BossStateMachine> AttackState;
    public StateBase<BossStateMachine> RecoilState;
    public StateBase<BossStateMachine> GroggyState;
    public StateBase<BossStateMachine> KnockbackState;
    public StateBase<BossStateMachine> GrabbedState;

    private void Awake()
    {
        if (boss == null)
        {
            boss = GetComponent<Boss1>();
        }

        MoveState = new BossMoveState(this);
        AttackState = new BossAttackState(this);
        RecoilState = new BossRecoilState(this);
        GroggyState = new BossGroggyState(this);
        KnockbackState = new SharedKnockbackState<BossStateMachine>(this);
        GrabbedState = new SharedGrabbedState<BossStateMachine>(this);
    }

    public bool IsCurrentState(object state)
    {
        return CurrentState == state;
    }

    public void ChangeState(StateBase<BossStateMachine> nextState, bool force = false)
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

    public void SetState(StateBase<BossStateMachine> nextState)
    {
        CurrentState = nextState ?? throw new ArgumentNullException(nameof(nextState));
    }

    public void UpdateState()
    {
        CurrentState?.UpdateState();
    }

    public void Move()
    {
        CurrentState?.Move();
    }

    public void ChangeToDefaultState()
    {
        BossGroggyState groggyState = GroggyState as BossGroggyState;
        if (groggyState != null && groggyState.IsGroggyActive)
        {
            ChangeState(GroggyState);
        }
        else
        {
            ChangeState(MoveState);
        }
    }
}
