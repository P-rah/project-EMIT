using System.Collections;
using UnityEngine;

public class Boss1AttackState : EnemyState
{
    private Boss1AttackStrategy currentStrategy;
    private Coroutine attackCoroutine;
    private bool isCancellable = false;
    private Boss_R1 bossEnemy;

    public Boss1AttackState(IEnemyStateMachine stateMachine) : base(stateMachine)
    {
        bossEnemy = enemy as Boss_R1; //타입 그대로 가져옴 수정 필요!
    }

    public void SetStrategy(Boss1AttackStrategy strategy)
    {
        currentStrategy = strategy;
    }

    public override void EnterState()
    {
        if (bossEnemy != null)
        {
            currentStrategy = bossEnemy.GetRandomAttackStrategy();
            bossEnemy.CurrentStrategy = currentStrategy;
        }

        if (currentStrategy == null)
        {
            stateMachine.ChangeState(stateMachine.ChaseState);
            return;
        }
        enemy.Flip();
        enemy.RootMovement();
        TryAttack();
    }

    public override void UpdateState()
    {
        if (EmitManager.Instance.isEmitting)
        {
            return;
        }
        if (IsAttacking()) return;
        stateMachine.ChangeState(stateMachine.IdleState);
    }

    public override void ExitState()
    {
        if (attackCoroutine != null)
        {
            enemy.StopCoroutine(attackCoroutine);
        }

        enemy.UnrootMovement();
        enemy.canChangeState = true;
        isCancellable = false;
        currentStrategy = null;

        if (bossEnemy != null)
        {
            bossEnemy.CurrentStrategy = null;
        }
    }

    public void TryAttack()
    {
        if (currentStrategy == null) return;

        enemy.canChangeState = false;
        attackCoroutine = enemy.StartCoroutine(AttackRoutineWrapper());
    }

    private IEnumerator AttackRoutineWrapper()
    {
        yield return currentStrategy.ExecuteAttackRoutine(enemy, setCancellable => isCancellable = setCancellable);

        attackCoroutine = null;
        enemy.UnrootMovement();
        enemy.canChangeState = true;
    }

    public bool IsAttacking() => attackCoroutine != null;
    public bool CanBeCancelled() => isCancellable;
    public void SetCancellable(bool value) => isCancellable = value;
}