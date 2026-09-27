using UnityEngine;

// 당기기 인터페이스
public interface IGrabbable
{
    int currentPower { get; }
    float grabForce { get; set; }

    bool transitFlipPower { get; set; }
    void ReceiveGrab();
}

public interface IGrabbableContext
{
    IGrabbable GrabbableEntity { get; }
    bool IsCurrentState(object state);

    void ChangeToDefaultState();
}

// 밀치기 인터페이스
public interface IKnockbackable
{
    Vector2 currentKnockBackForce { get; set; }

    int currentPower { get; }
    float knockbackRate { get; set; }
    float strongKnockbackRate { get; set; }
    float knockbackTime { get; set; }
    float HitRecoveryTime { get; set; }

    bool transitFlipPower { get; set; }

    void StartOverlapDetect();
    void StopOverlapDetect();
}

public interface IKnockbackableContext
{
    IKnockbackable KnockbackableEntity { get; }
    bool IsCurrentState(object state);

    void ChangeToDefaultState();
}

public interface IStateMachine
{
    bool IsCurrentState(object state);
    void ChangeToDefaultState();
}

public interface IStateControllable
{
    bool canChangeState { get; }
    void NotifyStateEntered(IState state);
}

public interface IState
{
    void EnterState();
    void UpdateState();
    void ExitState();
    void Move();
    public virtual bool CanBeCancelled()
    {
        return true;
    }
    virtual void OnReceiveKnockback() { }
    virtual void OnReceiveGrab() { }
}

public interface IEnemyStateMachine : IStateMachine
{
    EnemyBase EnemyBase { get; }

    // 공통 상태들 (제네릭 전혀 없음)
    IState CurrentState { get; }
    IState IdleState { get; }
    IState AttackState { get; }
    IState ChaseState { get; }
    IState DeadState { get; }

    void ChangeState(IState nextState, bool force = false);

    void UpdateState();
    void Move();
}