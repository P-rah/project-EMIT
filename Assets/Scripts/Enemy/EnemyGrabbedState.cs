using System.Collections;
using UnityEngine;

public class EnemyGrabbedState : EnemyState
{
    public EnemyGrabbedState(Enemy enemy) : base(enemy) { }

    private Coroutine grabRoutine;

    public override void EnterState()
    {
        Debug.Log(enemy.name + " entered Grab State");
        enemy.UnrootMovement();
        Grab();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!enemy.IsCurrentState(this) || isGrabbed())
        {
            return;
        }
    }

    public override void ExitState()
    {

    }

    public void Grab()
    {
        float offsetX = (enemy.target.position.x > enemy.transform.position.x) ? -1f : 1f;
        Vector2 targetPosition = new Vector2(enemy.target.position.x + offsetX, enemy.transform.position.y);

        enemy.canChangeState = false;
        grabRoutine = enemy.StartCoroutine(GrabRoutine(targetPosition));
   }

    IEnumerator GrabRoutine(Vector2 targetPosition)
    {
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

        enemy.rb.MovePosition(targetPosition);
        enemy.rb.linearVelocity = Vector2.zero;

        Debug.Log("당기기 끝");
        enemy.canChangeState = true;
        grabRoutine = null;
    }

    public bool isGrabbed()
    {
        return grabRoutine != null;
    }
}
