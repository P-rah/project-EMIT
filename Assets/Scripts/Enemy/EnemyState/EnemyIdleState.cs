using Unity.VisualScripting;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        enemy.enemyAnim.PlayIdle();
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
    }

    // public override void CheckState() { }

}
