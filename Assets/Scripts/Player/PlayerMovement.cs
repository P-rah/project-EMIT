using UnityEngine;
using System.Collections;
using EMIT.Utilities;

/*
 * 플레이어 이동 제어용 클래스
 */
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private PlayerState state;
    private DashEffectController dashEffectController;
    private PlayerDropDownPlatform playerDropDownPlatform;

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float moveSpeedWhileAttack = 0f;
    
    [Header("Jump Movement")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Slope Movement")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRayLength;
    [SerializeField] private bool isOnSlope;
    [SerializeField] private bool isExitingSlope;

    [Header("Platform Movement")]
    [SerializeField] private bool isDroppingDown;

    [Header("Dash Settings")]
    public float dashPower = 40f;
    public float dashTime = 0.1f;
    public float dashCooldown = 0.5f;
    private Vector2 facingDir = Vector2.right;
    private float initialGravityScale;
    [SerializeField] private bool canDash = true;

    public bool flippable = true;

    private void Awake()
    {
        rb = Player.Instance.Rb;
        input = GetComponent<PlayerInputHandler>();
        playerDropDownPlatform = GetComponent<PlayerDropDownPlatform>();
        state = Player.State;
        dashEffectController = Player.DashEffect;

        initialGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (Player.State.isDeath) return;
        HandleInputs();
        FlipPlayer();
    }

    private void HandleInputs()
    {
        if (input.ConsumeJump())
        {
            ExecuteJump();
        }
        if (input.ConsumeDash())
        {
            ExecuteDash();
        }
        if (input.ConsumeDown())
        {
            ExecuteDown();
        }
    }

    private void FixedUpdate()
    {
        if (Player.State.isDeath) return;
        if (playerDropDownPlatform.IsDropping)
    {
        SetGravityScale(initialGravityScale);
        // 탈출 즉시 이동을 멈추면서 아래로 떨어질 때 앞으로 튀어나가지 않도록 세팅
        if (Mathf.Abs(input.MoveInput.x) < 0.01f)
        {
            rb.linearVelocityX = 0f;
        }
        // 경사면 이동 처리에서 속도를 다시 투영하거나 y를 0으로 만들지 않도록 종료
        return;
    }

        // 경사면으로 대시하면 콜라이더 뚫음. 물리 업데이트를 안하니까.
        if (state.IsDashing) return;

        if (isExitingSlope && rb.linearVelocityY <= 0.1f && IsGrounded())
        {
            isExitingSlope = false;
        }
        // 수정: 경사면에서 이동하던 도중 공격을 하면 공중으로 뜸.
        if (state.IsAttackingSweep || state.IsAttackingGrab)
        {
            if (IsGrounded())
            {
                SetGravityScale(0f);
                rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeedWhileAttack, 0f);
            }
            else
            {
                SetGravityScale(initialGravityScale);
                rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeedWhileAttack, rb.linearVelocityY);
            }
        }
        else if (!isExitingSlope && IsOnSlope() && IsGrounded())
        {   
            // 경사면에 서 있다면, 경사면 방향으로 이동한다.
            Vector2 moveDir = ProjectOnLine(input.MoveInput, GetGroundNormal()).normalized;
            rb.linearVelocity = moveDir * moveSpeed;
            SetGravityScale(isOnSlope ? 0f : initialGravityScale);
        }
        else
        {   
            SetGravityScale(initialGravityScale);
            rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void ExecuteJump()
    {
        if (!IsGrounded())
        {
            return;
        }
        isExitingSlope = true;
        rb.gravityScale = initialGravityScale;
        rb.linearVelocityY = 0f;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        SoundManager.instance.PlaySFX(Player.Instance.jumpSound);
    }

    private void ExecuteDash()
    {
        if ( canDash && input.MoveInput.x !=0) StartCoroutine(DashRoutine());
    }

    private void ExecuteDown()
    {
        playerDropDownPlatform.TryDropDown();
        if (playerDropDownPlatform.IsDropping)
        {
            SetGravityScale(initialGravityScale);
        }
    }


    /// <summary>
    /// 원 범위 내에 있으면 땅 안에 있는 것으로 간주하고 true를 반환
    /// </summary>
    private bool IsGrounded()
    {
        if (Physics2D.OverlapCircle(
            groundCheckPos.position, groundCheckRayLength, groundLayer))
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 땅의 각도가 일정 이상이면 경사면으로 간주함.
    /// </summary>
    public bool IsOnSlope()
    {
        float angle = Vector2.Angle(Vector2.up, GetGroundNormal());
        isOnSlope =  angle > 0.05f;
        return isOnSlope; 
    }


    /// <summary>
    /// 아래쪽 땅의 각도를 체크함
    /// </summary>
    public Vector2 GetGroundNormal()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPos.position, Vector2.down, groundCheckRayLength, groundLayer); 
        if (hit.collider != null)
        {
            return hit.normal; 
        }
        
        return Vector2.up;
    }

    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    public static Vector2 ProjectOnLine(Vector2 vector, Vector2 normal)
    {
        if (normal.sqrMagnitude < Mathf.Epsilon)
            return vector;

        return vector
            - normal * (Vector2.Dot(vector, normal) / normal.sqrMagnitude);
    }

    private void FlipPlayer()
    {
        if (!flippable) return;

        if (Direction2D.IsOpposite(input.MoveInput, facingDir)) Flip();
        /*
        if (input.MoveInput.x > 0 && facingDirection < 0f) Flip();
        else if (input.MoveInput.x < 0 && facingDirection > 0f) Flip();
        */
    }

    public void Flip()
    {
        facingDir = -facingDir;
        transform.Rotate(0f, 180f, 0f);
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        state.IsDashing = true;
        // SoundManager.instance.PlaySFX(Player.Instance.dashSound);

        rb.gravityScale = 0f;

        rb.linearVelocity = facingDir * dashPower;
        dashEffectController.StartDashTrail();

        yield return new WaitForSeconds(dashTime);

        state.IsDashing = false;
        rb.gravityScale = initialGravityScale;
        dashEffectController.StopDashTrail();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void stopMove()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = initialGravityScale;
    }

    public void Animevent_playWalkSound()
    {
        SoundManager.instance.PlaySFX(Player.Instance.walkSound, 0.2f);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPos.position, 0.2f);
    }
}
