using UnityEngine;

public class BossMoveState : BossState
{
    private const float MoveDuration = 1f;

    private Vector2 startPosition;
    private Vector2 hoverPosition;
    private float moveElapsedTime;
    private bool canMove;
    private bool isMoveComplete;

    public BossMoveState(BossStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        boss.PrepareHovering();
        boss.ResetColor();
        startPosition = boss.Rigidbody.position;
        canMove = boss.TryGetRandomHoverPosition(out hoverPosition);
        moveElapsedTime = 0f;
        isMoveComplete = false;
    }

    public override void UpdateState()
    {
        if (isMoveComplete)
        {
            stateMachine.ChangeState(stateMachine.AttackState);
        }
    }

    public override void ExitState() { }

    public override void Move()
    {
        if (!canMove || isMoveComplete)
        {
            return;
        }

        moveElapsedTime += Time.fixedDeltaTime;
        float moveRatio = Mathf.Clamp01(moveElapsedTime / MoveDuration);
        Vector2 nextPosition = Vector2.Lerp(startPosition, hoverPosition, moveRatio);

        boss.Rigidbody.MovePosition(nextPosition);
        isMoveComplete = moveRatio >= 1f;
    }
}
