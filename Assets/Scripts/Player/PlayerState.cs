using UnityEngine;
/*
 * 플레이어 상태 관리용 클래스
 */
public enum MoveState
{
    Idle,
    Running,
    Jumping,
    Falling,
    Dashing,
}
public enum AttackState
{
    notAttacking,
    AttackingSweep,
    AttackingGrap
}
public enum EmitState
{
    emiting,
    notEmiting
}

public class PlayerState : MonoBehaviour
{
    public MoveState CurrentMoveState { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; set; }

    public AttackState CurrentAttackState { get; private set; }
    public bool isAttackingSweep { get; set; }
    public bool isAttackingGrap { get; set; }

    public bool isEmiting { get; set; }

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckGrounded();
        DetermineMoveState();
        DetermineAttackState();
    }

    private void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void DetermineMoveState()
    {
        MoveState newMoveState = CurrentMoveState;

        if (IsDashing)
        {
            newMoveState = MoveState.Dashing;
        }
        else if (IsGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                newMoveState = MoveState.Running;
            else
                newMoveState = MoveState.Idle;
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f)
                newMoveState = MoveState.Jumping;
            else
                newMoveState = MoveState.Falling;
        }

        if (newMoveState != CurrentMoveState)
        {
            ChangeState(newMoveState);
        }
    }

    private void DetermineAttackState()
    {
        AttackState newAttackState = CurrentAttackState;

        if (isAttackingSweep)
        {
            newAttackState = AttackState.AttackingSweep;
        }
        else if(isAttackingGrap)
        {
            newAttackState = AttackState.AttackingGrap;
        }
        else newAttackState = AttackState.notAttacking;

        if (newAttackState != CurrentAttackState)
        {
            ChangeState(newAttackState);
        }
    }

    private void ChangeState(MoveState newMoveState)
    {
        CurrentMoveState = newMoveState;
    }

    private void ChangeState(AttackState newAttackState)
    {
        CurrentAttackState = newAttackState;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}
