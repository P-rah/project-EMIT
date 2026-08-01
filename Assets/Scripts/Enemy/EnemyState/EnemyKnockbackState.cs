using System.Collections;
using UnityEngine;

public class EnemyKnockbackState : EnemyState
{
    public EnemyKnockbackState(EnemyStateMachine stateMachine) : base(stateMachine) { }
    private Coroutine knockbackRoutine;

    public override void EnterState()
    {
        enemy.UnrootMovement();
        Knockback();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!stateMachine.IsCurrentState(this) || isKnockbacking())
        {
            return;
        }
    }

    public override void ExitState()
    {

    }

    public void Knockback()
    {
        enemy.canChangeState = false;
        enemy.rb.AddForce(enemy.currentKnockBackForce * enemy.knockbackRate, ForceMode2D.Impulse);
        if((enemy.currentKnockBackForce.x > 0f) ==  (enemy.isFacingLeft))
            enemy.enemyAnim.PlayHitFront();
        else enemy.enemyAnim.PlayHitBack();

        knockbackRoutine = enemy.StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        enemy.startDragEffect();
        yield return new WaitForSeconds(enemy.knockbackTime);
        enemy.endDragEffect();
        enemy.canChangeHitState = true;
        enemy.rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(enemy.HitRecoveryTime);
        enemy.canChangeHitState = false;
        enemy.canChangeState = true;
        knockbackRoutine = null;
    }

    public void stopKnockbackRoutine()
    {
        if (knockbackRoutine != null)
        {
            enemy.canChangeHitState = false;
            enemy.StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }
    }

    public bool isKnockbacking()
    {
        return knockbackRoutine != null;
    }
}
