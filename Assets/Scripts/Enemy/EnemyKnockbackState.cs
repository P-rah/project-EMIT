using System.Collections;
using UnityEngine;

public class EnemyKnockbackState : EnemyState
{
    public EnemyKnockbackState(Enemy enemy) : base(enemy) {}
    private Coroutine knockbackRoutine;

    public override void EnterState()
    {
        Debug.Log(enemy.name + " entered Knockback State");
        enemy.UnrootMovement();
        knockback();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!enemy.IsCurrentState(this) || isKnockbacking())
        {
            return;
        }
    }

    public override void ExitState()
    {

    }

    public void knockback()
    {
        enemy.canChangeState = false;
        enemy.rb.AddForce(enemy.currentKnockBackForce * enemy.knockbackRate, ForceMode2D.Impulse);
        knockbackRoutine = enemy.StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        yield return new WaitForSeconds(enemy.knockbackTime);
        enemy.canChangeState = true;
    }
    public bool isKnockbacking()
    {
        return knockbackRoutine != null;
    }
}
