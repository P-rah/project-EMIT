using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy, EnemyAnimator enemyAnim) : base(enemy, enemyAnim) { }

    public override void EnterState()
    {
        enemyAnim.PlayWalk();
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {

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
