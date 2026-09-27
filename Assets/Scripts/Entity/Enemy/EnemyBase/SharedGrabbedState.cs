using System.Collections;
using UnityEngine;

public class SharedGrabbedState : EnemyState
{
    public SharedGrabbedState(IEnemyStateMachine stateMachine) : base(stateMachine)
    {
    }

    private Coroutine grabRoutine;

    public override void EnterState()
    {
        Debug.Log("그랩 상태 돌입");
        enemy.canChangeState = false;
        enemy.UnrootMovement();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!stateMachine.IsCurrentState(this) || isGrabbed()) return;
    }

    public override void ExitState()
    {
        stopGrapRoutine();
    }

    public void Grab()
    {
        float offsetX = (enemy.target.position.x > enemy.transform.position.x) ? -0.5f : 0.5f;
        Vector2 targetPosition = new Vector2(enemy.target.position.x + offsetX, enemy.transform.position.y);

        grabRoutine = enemy.StartCoroutine(GrabRoutine(targetPosition));
    }

    IEnumerator GrabRoutine(Vector2 targetPosition)
    {
        enemy.startDragEffect();
        enemy.rb.linearVelocity = Vector2.zero;

        while (Vector2.Distance(enemy.rb.position, targetPosition) > 0.1f)
        {
            Vector2 nextPosition = Vector2.MoveTowards(
                enemy.rb.position,
                targetPosition,
                enemy.grabForce * Time.fixedDeltaTime
            );

            enemy.rb.MovePosition(nextPosition);
            yield return new WaitForFixedUpdate();
        }

        enemy.endDragEffect();
        enemy.rb.MovePosition(targetPosition);
        enemy.rb.linearVelocity = Vector2.zero;
        enemy.canChangeHitState = true;

        yield return new WaitForSeconds(enemy.HitRecoveryTime);

        enemy.canChangeHitState = false;
        enemy.canChangeState = true;
        grabRoutine = null;

        stateMachine.ChangeToDefaultState();
    }

    public void stopGrapRoutine()
    {
        if (grabRoutine != null)
        {
            enemy.canChangeHitState = false;
            enemy.canChangeState = true;
            enemy.StopCoroutine(grabRoutine);
            grabRoutine = null;
        }
    }

    public bool isGrabbed() => grabRoutine != null;
}