using System;
using UnityEngine;
/*
    플레이어 상태 관리용 클래스
    
    현재 문제점
    - 공중에서 대시, 떨어지면서 공격 등 중복될 수 있는 state가 별개의 상태로 구분되어있음.
    - 또한, PlayerMovement에서 Attack 중 Speed가 설정된 이상 Running과 Attacking도 구분되는 상태임.
      지금은 공격 중 이속을 꺼놔서 괜찮지만, 이속이 0이 아니라면 달리면서 공격 등의 상태도 명확히 표현 불가
    - 그래서 CurrentState가 중첩·혼재 가능한 상태들을 지금으로써는 정확히 표현하기 힘듦
*/
public enum State
{
    Idle,
    Running,
    Jumping,
    Falling,
    Dashing,
    AttackingSweep,
    AttackingGrab,
    Death
}

public class PlayerState : MonoBehaviour
{
    [SerializeField] private string currentStateString; // 인스펙터 표시용 (임시)
    public State CurrentState { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; set; }
    public bool IsAttackingSweep { get; set; }
    public bool IsAttackingGrab { get; set; }
    public bool isDeath { get; set; }

    private bool isFalling; //공중에 있다가 착지 감지용

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
        currentStateString = CurrentState.ToString();
    }

    private void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (!IsGrounded) isFalling = true;
        else if(isFalling)
        {
            // AudioManager.Instance.PlaySFX(Player.Instance.landSound);
            isFalling = false;
        }
    }

    private void DetermineState()
    {
        State newState = CurrentState;

        if (isDeath) newState = State.Death;
        else if (IsAttackingSweep) newState = State.AttackingSweep;
        else if (IsAttackingGrab) newState = State.AttackingGrab;
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
