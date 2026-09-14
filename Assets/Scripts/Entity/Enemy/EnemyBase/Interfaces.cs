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
    void NotifyStateEntered(StateBase<EnemyStateMachine> state);
}