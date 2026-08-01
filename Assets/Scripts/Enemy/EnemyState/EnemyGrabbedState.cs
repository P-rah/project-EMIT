using System.Collections;
using UnityEngine;

public class EnemyGrabbedState : EnemyState
{
    public EnemyGrabbedState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    private Coroutine grabRoutine;

    private int intensity;

    public override void EnterState()
    {
        enemy.canChangeState = false;
        enemy.UnrootMovement();
        intensity = 1;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!stateMachine.IsCurrentState(this) || isGrabbed())
        {
            return;
        }
    }

    public override void ExitState()
    {

    }

    public void Grab()
    {
        float offsetX = (enemy.target.position.x > enemy.transform.position.x) ? -0.5f : 0.5f;
        Vector2 targetPosition = new Vector2(enemy.target.position.x + offsetX, enemy.transform.position.y);

        if ((offsetX > 0f) == (enemy.isFacingLeft))
            enemy.enemyAnim.PlayHitBack();
        else enemy.enemyAnim.PlayHitFront();

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
    }

    public void stopGrapRoutine()
    {
        if (grabRoutine != null)
        {
            enemy.canChangeHitState = false;
            enemy.StopCoroutine(grabRoutine);
            grabRoutine = null;
        }
    }

    public bool isGrabbed()
    {
        return grabRoutine != null;
    }
}
