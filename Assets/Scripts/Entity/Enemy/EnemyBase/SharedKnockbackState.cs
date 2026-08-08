using System.Collections;
using UnityEngine;

public class SharedKnockbackState<T> : StateBase<T> where T : IKnockbackableContext
{
    private IKnockbackable entity;

    public SharedKnockbackState(T owner) : base(owner)
    {
        this.entity = owner.KnockbackableEntity;
    }

    private Coroutine knockbackRoutine;

    public override void EnterState()
    {
        Debug.Log("넉백됨");
        entity.UnrootMovement();
        Knockback();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!owner.IsCurrentState(this) || isKnockbacking())
        {
            return;
        }
    }

    public override void ExitState()
    {
        stopKnockbackRoutine();
    }

    public void Knockback()
    {
        entity.canChangeState = false;
        entity.rb.AddForce(entity.currentKnockBackForce * entity.knockbackRate, ForceMode2D.Impulse);
        knockbackRoutine = entity.StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        entity.startDragEffect();
        yield return new WaitForSeconds(entity.knockbackTime);

        entity.endDragEffect();
        entity.canChangeHitState = true;
        entity.rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(entity.HitRecoveryTime);

        entity.canChangeHitState = false;
        entity.canChangeState = true;
        knockbackRoutine = null;

        owner.ChangeToDefaultState();
    }

    public void stopKnockbackRoutine()
    {
        if (knockbackRoutine != null)
        {
            entity.canChangeHitState = false;
            entity.canChangeState = true;

            entity.StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }
    }

    public bool isKnockbacking()
    {
        return knockbackRoutine != null;
    }
}
