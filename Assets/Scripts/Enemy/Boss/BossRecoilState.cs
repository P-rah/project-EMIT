using UnityEngine;

public class BossRecoilState : BossState
{
    private const float RecoilDuration = 5f;

    private float moveTime;

    public BossRecoilState(BossStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        boss.PrepareHovering();
        boss.SetRecoilColor();
        moveTime = Time.time + RecoilDuration;
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
        boss.ResetColor();
    }
}
