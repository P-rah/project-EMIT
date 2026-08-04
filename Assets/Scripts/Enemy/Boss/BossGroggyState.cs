using UnityEngine;

public class BossGroggyState : BossState
{
    private const float GroggyDuration = 5f;

    private float moveTime;

    public BossGroggyState(BossStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        boss.EnterGroggy();
        moveTime = Time.time + GroggyDuration;
    }

    public override void UpdateState()
    {
        if (Time.time >= moveTime)
        {
            stateMachine.ChangeState(stateMachine.MoveState);
        }
    }

    public override void ExitState()
    {
        boss.ExitGroggy();
    }
}
