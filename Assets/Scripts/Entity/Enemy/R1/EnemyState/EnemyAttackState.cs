using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(IEnemyStateMachine stateMachine) : base(stateMachine) { }

    private Coroutine attackCoroutine;
    private bool isCancellable = false;

    public override void EnterState()
    {
        enemy.RootMovement();
        TryAttack();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!stateMachine.IsCurrentState(this) || IsAttacking())
        {
            return;
        }
        enemy.Flip();
        if (EmitManager.Instance.isEmitting)
        {
            return;
        }
        TryAttack();
    }

    public override void ExitState()
    {
        if (attackCoroutine != null)
        {
            enemy.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            enemy.UnrootMovement();
            enemy.canChangeState = true;
        }
    }

    public void TryAttack()
    {   
        enemy.canChangeState = false;
        attackCoroutine = enemy.StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isCancellable = true;
        enemy.NotifyAttackWindupStarted();

        yield return new WaitForSeconds(enemy.startUpTime);

        // 본공격
        enemy.NotifyAttackStrikeStarted();
        isCancellable = false;
        CameraShake.Instance.ShakeCamera(10f,0.2f);
        if (enemy.myAudioSource != null && enemy.attackSound != null)
        {
            enemy.myAudioSource.PlayOneShot(enemy.attackSound);
        }

        // 후딜레이 시간
        yield return new WaitForSeconds(enemy.recoveryTime);
        attackCoroutine = null;
        enemy.UnrootMovement();
        enemy.canChangeState = true;
    }

    public bool IsAttacking()
    {
        return attackCoroutine != null;
    }

    public bool CanBeCancelled()
    {
        return isCancellable;
    }

    public void SetCancellable(bool value)
    {
        isCancellable = value;
    }
}
