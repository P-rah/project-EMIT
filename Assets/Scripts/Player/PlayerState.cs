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
    AttackingGrap
}

public enum EmitState
{
    emiting,
    notEmiting
}

public class PlayerState : MonoBehaviour
{
    public State CurrentState { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; set; }
    public bool isAttackingSweep { get; set; }
    public bool isAttackingGrap { get; set; }

    public bool attacked = false;

    public EmitState CurrentEmitState { get; private set; }
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
        DetermineState();
        DetermineEmitState();
    }

    private void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void DetermineState()
    {
        State newState = CurrentState;


        if(isAttackingSweep) newState = State.AttackingSweep;
        else if (isAttackingSweep) newState = State.AttackingGrap;
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
            Debug.Log(newState);
        }
    }

    private void DetermineEmitState()
    {
        EmitState newEmitState = CurrentEmitState;

        if (isEmiting) newEmitState = EmitState.emiting;
        else newEmitState = EmitState.notEmiting;

        if (newEmitState != CurrentEmitState)
        {
            ChangeState(newEmitState);
        }
    }

    private void ChangeState(State newState)
    {
        CurrentState = newState;
    }

    private void ChangeState(EmitState newEmitState)
    {
        CurrentEmitState = newEmitState;
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
