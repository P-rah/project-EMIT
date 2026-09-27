using UnityEngine;

public class EnemyDeadState : EnemyState
{
    private DeathEffectType type;

    public EnemyDeadState(IEnemyStateMachine stateMachine, DeathEffectType type) : base(stateMachine)
    {
        this.type = type;
    }

    public override void EnterState()
    {
        Debug.Log("Enemy is Dead");
        enemy.Flip();
        EffectManager.Instance?.PlayDeathEffect(type, enemy.transform.position, enemy.isFacingLeft);
    }

    public override void ExitState()
    {
        // 적이 죽은 이후 실행시킬 로직

    }
}
