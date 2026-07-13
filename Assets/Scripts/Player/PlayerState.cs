using UnityEngine;
/*
 * 플레이어 상태 관리용 클래스
 */
public enum State
{
    Idle,
    Running,
    Jumping,
    Falling,
    Dashing,
    AttackingSweep,
    AttackingGrab
}

public class PlayerState : MonoBehaviour
{
    public State CurrentState { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; set; }
    public bool isAttackingSweep { get; set; }
    public bool isAttackingGrab { get; set; }

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = Player.Instance.Rb;
    }

    private void Update()
    {
        CheckGrounded();
        DetermineState();
    }

    private void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void DetermineState()
    {
        State newState = CurrentState;

        if (isAttackingSweep) newState = State.AttackingSweep;

        else if (isAttackingGrab) newState = State.AttackingGrab;
        else if (IsDashing) newState = State.Dashing;
        else if (IsGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                newState = State.Running;
            else
                newState = State.Idle;
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f)
                newState = State.Jumping;
            else
                newState = State.Falling;
        }

        if (newState != CurrentState)
        {
            ChangeState(newState);
        }
    }

    private void ChangeState(State newState)
    {
        CurrentState = newState;
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
