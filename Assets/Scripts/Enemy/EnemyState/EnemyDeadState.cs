using UnityEngine;

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) {}

    public override void EnterState()
    {
        Debug.Log("Enemy is Dead");
        enemy.Flip();
        // EffectManager.Instance.PlayDeathEffect(enemy.transform.position, enemy.isFacingLeft);
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        // 적이 죽은 이후 실행시킬 로직

    }

    public override void CheckState()
    {

    }
}
