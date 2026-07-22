using UnityEngine;
using System.Collections;
using NUnit.Framework.Interfaces;
using UnityEngine.UIElements;

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
    public float moveSpeedWhileAttack = 5f;
    
    [Header("Jump Movement")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Slope Movement")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private bool isOnSlope;
    [SerializeField] private bool isExitingSlope;


    [Header("Dash Settings")]
    public float dashPower = 40f;
    public float dashTime = 0.1f;
    public float dashCooldown = 0.5f;
    private float facingDirection = 1f;
    private float originalGravity;
    [SerializeField] private bool canDash = true;

    public bool flippable = true;

    private void Awake()
    {
        rb = Player.Instance.Rb;
        input = GetComponent<PlayerInputHandler>();
        playerDropDownPlatform = GetComponent<PlayerDropDownPlatform>();
        state = Player.State;
        dashEffectController = Player.DashEffect;

        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
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
            playerDropDownPlatform.TryDropDown();
        }
    }

    private void FixedUpdate()
    {
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
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeedWhileAttack, 0f);
            }
            else
            {
                rb.gravityScale = originalGravity;
                rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeedWhileAttack, rb.linearVelocityY);
            }
        }
        else if (!isExitingSlope && IsOnSlope() && IsGrounded())
        {   
            // 경사면에 서 있다면, 경사면 방향으로 이동한다.
            Vector2 moveDir = ProjectOnLine(input.MoveInput, GetGroundNormal()).normalized;
            rb.linearVelocity = moveDir * moveSpeed;
            rb.gravityScale = isOnSlope ? 0f : originalGravity;
        }
        else
        {   
            rb.gravityScale = originalGravity;
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
        rb.gravityScale = originalGravity;
        rb.linearVelocityY = 0f;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void ExecuteDash()
    {
        if ( canDash && input.MoveInput.x !=0) StartCoroutine(DashRoutine());
    }

    private bool IsGrounded()
    {
        if (Physics2D.OverlapCircle(groundCheckPos.position, 0.2f, groundLayer))
        {
            return true;
        }
        return false;
    }

    private bool IsOnSlope()
    {
        float angle = Vector2.Angle(Vector2.up, GetGroundNormal());
        isOnSlope =  angle > 0.05f;
        return isOnSlope; 
    }

    public Vector2 GetGroundNormal()
    {
        float rayLength = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPos.position, Vector2.down, rayLength, groundLayer); 
        if (hit.collider != null)
        {
            return hit.normal; 
        }
        
        return Vector2.up;
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

        if (input.MoveInput.x > 0 && facingDirection < 0f) Flip();
        else if (input.MoveInput.x < 0 && facingDirection > 0f) Flip();
    }

    public void Flip()
    {
        facingDirection = -facingDirection;
        transform.Rotate(0f, 180f, 0f);
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        state.IsDashing = true;

        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(facingDirection * dashPower, 0f);
        dashEffectController.StartDashTrail();

        yield return new WaitForSeconds(dashTime);

        state.IsDashing = false;
        rb.gravityScale = originalGravity;
        dashEffectController.StopDashTrail();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        
    }
}
