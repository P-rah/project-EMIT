using Unity.VisualScripting;

public abstract class EnemyState
{
    /// <summary>
    /// 책임 및 기능: 각 State에서 Enemy가 움직이는 방식
    /// </summary>
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;
    protected EnemyAnimator enemyAnim;

    public EnemyState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.enemy = stateMachine.Owner;
        this.enemyAnim = enemy.enemyAnim;
    }

    public abstract void EnterState();

    public virtual void UpdateState()
    {
        CheckState();
    }

    public virtual void CheckState()
    {
        if (enemy.isPlayerInAttackRange())
        {
            stateMachine.ChangeState(stateMachine.AttackState);
            return;
        }
        if (enemy.isPlayerInVision())
        {
            stateMachine.ChangeState(stateMachine.ChaseState);
            return;
        }
        stateMachine.ChangeState(stateMachine.IdleState);
    }

    public abstract void ExitState();

    public virtual void Move() {}
}
