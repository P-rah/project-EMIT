using UnityEngine;

public class BossMoveState : StateBase<BossStateMachine>
{
    private Boss1 boss;

    private const float MoveDuration = 1f;

    private Vector2 startPosition;
    private Vector2 hoverPosition;
    private float moveElapsedTime;

    private bool isMoveComplete;

    public BossMoveState(BossStateMachine owner) : base(owner)
    {
        boss = owner.boss;
    }

    public override void EnterState()
    {
        boss.PrepareHovering();
        startPosition = boss.rb.position;
        moveElapsedTime = 0f;

        if (!boss.TryGetRandomHoverPosition(out hoverPosition))
        {
            isMoveComplete = true;
        }
        else
        {
            isMoveComplete = false;
        }
    }

    public override void UpdateState()
    {
        if (isMoveComplete)
        {
            owner.ChangeState(owner.AttackState);
        }
    }

    public override void ExitState()
    {

    }

    public override void Move()
    {
        if (isMoveComplete)
        {
            return;
        }

        moveElapsedTime += Time.fixedDeltaTime;
        float moveRatio = Mathf.Clamp01(moveElapsedTime / MoveDuration);

        float easedRatio = Mathf.SmoothStep(0f, 1f, moveRatio);

        Vector2 nextPosition = Vector2.Lerp(startPosition, hoverPosition, easedRatio);

        boss.rb.MovePosition(nextPosition);

        if (moveRatio >= 1f)
        {
            isMoveComplete = true;
        }
    }

    public override bool CanBeCancelled() => false;
}