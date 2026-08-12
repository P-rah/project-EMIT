using UnityEngine;

public class BossRecoilState : StateBase<BossStateMachine>
{
    private Boss1 boss;

    private const float RecoilDuration = 2f;

    private float moveTime;

    public BossRecoilState(BossStateMachine owner) : base(owner)
    {
        boss = owner.boss;
    }

    public override void EnterState()
    {
        boss.PrepareHovering();
        moveTime = Time.time + RecoilDuration;
    }

    public override void UpdateState()
    {
        if (Time.time >= moveTime)
        {
            owner.ChangeState(owner.MoveState);
        }
    }

    public override void ExitState()
    {
    }

    public override bool CanBeCancelled()
    {
        return false;
    }
}
