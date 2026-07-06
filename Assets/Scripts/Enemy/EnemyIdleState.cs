using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(Enemy enemy) : base(enemy) { }

    public override void EnterState()
    {
        Debug.Log(enemy.name + " entered Idle State");
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        Debug.Log(enemy.name + " exited Idle State");
    }

    // public override void CheckState() { }

}
