using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyGrappedState : EnemyState
{
    public EnemyGrappedState(Enemy enemy) : base(enemy) { }

    private Coroutine grapRoutine;

    public override void EnterState()
    {
        Debug.Log(enemy.name + " entered grap State");
        enemy.UnrootMovement();
        grap();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!enemy.IsCurrentState(this) || isGrapping())
        {
            return;
        }
    }

    public override void ExitState()
    {

    }

    public void grap()
    {
        float offsetX = (enemy.target.position.x > enemy.transform.position.x) ? -1f : 1f;
        Vector2 targetPosition = new Vector2(enemy.target.position.x + offsetX, enemy.transform.position.y);

        enemy.canChangeState = false;
        grapRoutine = enemy.StartCoroutine(GrapRoutine(targetPosition));
   }

    IEnumerator GrapRoutine(Vector2 targetPosition)
    {
        while (Vector2.Distance(enemy.transform.position, targetPosition) > 0.1f)
        {
            enemy.transform.position = Vector2.MoveTowards(
                enemy.transform.position,
                targetPosition,
                enemy.grapForce * Time.deltaTime
            );
            yield return null;
        }

        Debug.Log("당기기 끝");
        enemy.transform.position = targetPosition;
        enemy.canChangeState = true;
    }

    public bool isGrapping()
    {
        return grapRoutine != null;
    }
}
