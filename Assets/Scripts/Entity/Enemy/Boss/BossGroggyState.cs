using UnityEngine;

public class BossGroggyState : StateBase<BossStateMachine>
{
    private Boss1 boss;
    private float groggyTimer;
    private const float maxGroggyTime = 5f;

    public bool IsGroggyActive { get; private set; }

    public BossGroggyState(BossStateMachine owner) : base(owner)
    {
        boss = owner.boss;
    }

    public override void EnterState()
    {
        boss.EnterGroggy();
        groggyTimer += 0.5f;
        if (!IsGroggyActive)
        {
            groggyTimer = 0f;
            IsGroggyActive = true;
        }
    }

    public override void UpdateState()
    {
        groggyTimer += Time.deltaTime;

        if (groggyTimer >= maxGroggyTime)
        {
            IsGroggyActive = false;
            owner.ChangeState(owner.MoveState);
        }
    }

    public override void ExitState()
    {
        
    }

    public override bool CanBeCancelled()
    {
        return true;
    }

    public override void OnReceiveKnockback()
    {
        owner.ChangeState(owner.KnockbackState, true);
    }

    public override void OnReceiveGrab()
    {
        owner.ChangeState(owner.GrabbedState, true);
    }
}
