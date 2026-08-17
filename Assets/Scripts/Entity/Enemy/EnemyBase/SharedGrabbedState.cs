using System.Collections;
using UnityEngine;

public class SharedGrabbedState<T> : StateBase<T> where T : IGrabbableContext
{
    private IGrabbable entity;

    public SharedGrabbedState(T owner) : base(owner)
    {
        this.entity = owner.GrabbableEntity;
    }

    private Coroutine grabRoutine;

    public override void EnterState()
    {
        Debug.Log("그랩됨");
        entity.canChangeState = false;
        entity.UnrootMovement();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!owner.IsCurrentState(this) || isGrabbed())
        {
            return;
        }
    }

    public override void ExitState()
    {
        stopGrapRoutine();
    }

    public void Grab()
    {
        float offsetX = (entity.target.position.x > entity.transform.position.x) ? -0.5f : 0.5f;
        Vector2 targetPosition = new Vector2(entity.target.position.x + offsetX, entity.transform.position.y);

        //entity.NotifyGrabStarted(offsetX);
        grabRoutine = entity.StartCoroutine(GrabRoutine(targetPosition));
    }

    IEnumerator GrabRoutine(Vector2 targetPosition)
    {
        entity.startDragEffect();
        entity.rb.linearVelocity = Vector2.zero;

        while (Vector2.Distance(entity.rb.position, targetPosition) > 0.1f)
        {
            Vector2 nextPosition = Vector2.MoveTowards(
                entity.rb.position,
                targetPosition,
                entity.grabForce * 0.3f * Time.fixedDeltaTime
            );

            entity.rb.MovePosition(nextPosition);

            yield return new WaitForFixedUpdate();
        }

        entity.endDragEffect();
        entity.rb.MovePosition(targetPosition);
        entity.rb.linearVelocity = Vector2.zero;
        entity.canChangeHitState = true;

        yield return new WaitForSeconds(entity.HitRecoveryTime);

        entity.canChangeHitState = false;
        entity.canChangeState = true;
        grabRoutine = null;

        owner.ChangeToDefaultState();
    }

    public void stopGrapRoutine()
    {
        if (grabRoutine != null)
        {
            entity.canChangeHitState = false;
            entity.canChangeState = true;
            entity.StopCoroutine(grabRoutine);
            grabRoutine = null;
        }
    }

    public bool isGrabbed()
    {
        return grabRoutine != null;
    }
}
