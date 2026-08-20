using System.Collections;
using UnityEngine;

public class SharedKnockbackState<T> : StateBase<T> where T : IStateMachine
{
    private EnemyBase enemy;

    public SharedKnockbackState(T owner, EnemyBase enemy) : base(owner)
    {
        this.enemy = enemy;
    }

    private Coroutine knockbackRoutine;

    public override void EnterState()
    {
        enemy.UnrootMovement();
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
        enemy.canChangeState = false;
        if (enemy.transitFlipPower)
        {
            enemy.rb.AddForce(enemy.currentKnockBackForce * enemy.strongKnockbackRate, ForceMode2D.Impulse);
            enemy.StartOverlapDetect();
        }
        else
        {
            enemy.rb.AddForce(enemy.currentKnockBackForce * enemy.knockbackRate, ForceMode2D.Impulse);
        }

        knockbackRoutine = enemy.StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        enemy.startDragEffect();
        yield return new WaitForSeconds(enemy.knockbackTime);

        enemy.endDragEffect();
        enemy.canChangeHitState = true;
        enemy.rb.linearVelocity = Vector2.zero;
        enemy.StopOverlapDetect();

        yield return new WaitForSeconds(enemy.HitRecoveryTime);

        enemy.canChangeHitState = false;
        enemy.canChangeState = true;
        knockbackRoutine = null;

        owner.ChangeToDefaultState();
    }

    public void stopKnockbackRoutine()
    {
        if (knockbackRoutine != null)
        {
            enemy.canChangeHitState = false;
            enemy.canChangeState = true;
            enemy.StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }
    }

    public bool isKnockbacking()
    {
        return knockbackRoutine != null;
    }
}
