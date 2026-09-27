using UnityEngine;

public class Boss1IdleState : EnemyState
{
    public Boss1IdleState(IEnemyStateMachine stateMachine) : base(stateMachine) { }

    private float idleTimer;
    private float idleDuration = 0.8f; 

    public override void EnterState()
    {
        idleTimer = 0f;
        enemy.flipable = false;
        enemy.RootMovement();
    }

    public override void UpdateState()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            CheckState();
        }
    }

    public override void ExitState()
    {
        enemy.flipable = true;
        enemy.UnrootMovement();
    }

    public override void Move()
    {
    }
}