using UnityEngine;

public class BossDeadState : EnemyDeadState
{
    public BossDeadState(IEnemyStateMachine stateMachine, DeathEffectType type) : base(stateMachine, type)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        Debug.Log("Boss is Dead");

        EventManager.TriggerEvent("BossDead");
    }
}
