using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy) : base(enemy) {}

    public override void EnterState()
    {
        Debug.Log(enemy.name + " entered Chase State");
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        Debug.Log(enemy.name + " exited Chase State");
    }

    // RigidBody 기반 Movement이므로 Enemy.FixedUpdate()에서 해당 함수 호출됨
    public override void Move()
    {
        if (enemy.rb == null)
        {
            Debug.LogError("Enemy requires RigidBody2D");
            return;
        }

        enemy.rb.linearVelocity = new Vector2(enemy.baseSpeed, 0) * enemy.front;
    }
}
