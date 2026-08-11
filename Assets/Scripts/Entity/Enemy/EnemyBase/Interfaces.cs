using UnityEngine;
using System.Collections;

// 당기기 인터페이스
public interface IGrabbable
{
    Transform transform { get; }
    Transform target { get; set; }
    Rigidbody2D rb { get; set; }

    float grabForce { get; set; }
    float HitRecoveryTime { get; set; }
    bool canChangeState { get; set; }
    bool canChangeHitState { get; set; }

    void UnrootMovement();
    void startDragEffect();
    void endDragEffect();
    void ReceiveGrab();

    Coroutine StartCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
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
    Rigidbody2D rb { get; set; }
    Vector2 currentKnockBackForce { get; set; }

    float knockbackRate { get; set; }
    float knockbackTime { get; set; }
    float HitRecoveryTime { get; set; }
    bool canChangeState { get; set; }
    bool canChangeHitState { get; set; }

    void UnrootMovement();
    void startDragEffect();
    void endDragEffect();

    Coroutine StartCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
}

public interface IKnockbackableContext
{
    IKnockbackable KnockbackableEntity { get; }
    bool IsCurrentState(object state);

    void ChangeToDefaultState();
}