using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateMachine owner) : base(owner)
    {
    }

    public override void EnterState()
    {
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

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
        // 추후 PlayerMovement와 같이 Slope 이동 가능하도록 수정
        enemy.rb.linearVelocity = new Vector2(enemy.baseSpeed, 0) * enemy.targetDir;
    }
}
