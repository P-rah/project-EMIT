using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(Enemy enemy, EnemyAnimator enemyAnim) : base(enemy, enemyAnim) { }
    private Coroutine attackCoroutine;

    private bool isCancellable = false;

    public override void EnterState()
    {
        Debug.Log(enemy.name + " entered Attack State");
        enemyAnim.PlayAttack();
        enemy.RootMovement();
        TryAttack();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!enemy.IsCurrentState(this) || IsAttacking())
        {
            return;
        }
        if (EmitManager.Instance.isEmitting)
        {
            return;
        }
        TryAttack();
    }

    public override void ExitState()
    {
        Debug.Log(enemy.name + " exited Attack State");

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
        // 선딜레이 시간
        Debug.Log("Charging Attack...");
        isCancellable = true;
        yield return new WaitForSeconds(enemy.startUpTime);
        // 선딜레이 시간 동안에 넉백을 받았다면 TryAttack이 실패하고 KnockBackState가 되어야 함
        
        // 본공격
        Debug.Log("Enemy Attacked!");
        isCancellable = false;
        // 공격 판정이 될 콜라이더 활성화
        // 
        // 콜라이더 비활성화
        // 후딜레이 시간
        Debug.Log("Recovering from Attack...");
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
}
