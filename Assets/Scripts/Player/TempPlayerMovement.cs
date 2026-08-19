using UnityEngine;
using System.Collections;
using EMIT.Utilities;

public class TempPlayerMovement : MonoBehaviour
{
#region 변수 선언
    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private PlayerState state;
    private DashEffectController dashEffectController;
    private PlayerDropDownPlatform dropDownPlatform;

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float moveSpeedWhileAttack = 0f;

    [Header("Jump Movement")]
    [SerializeField] private float jumpForce = 15f;
    [Tooltip("대충 점프 로직을 위해 넣은 값 (필요시 문의)")]
    [SerializeField] private float jumpProtectionDuration = 0.1f;
    [SerializeField] private bool isJumpProtected;
    private bool jumpRequested;

    [Header("Slope Movement")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask slideMovementLayer; 
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRayLength = 0.3f;
    [SerializeField] private bool checkGroundedByRay = true;
    [SerializeField] private Rigidbody2D.SlideMovement slideMovement;

    private CapsuleCollider2D playerBodyCollider;

    [Header("Dash Settings")]
    public float dashSpeed = 40f;
    public float dashTime = 0.1f;
    public float dashCooldown = 0.5f;
    [SerializeField] private bool isDashing;
    [SerializeField] private bool canDash = true;
    private Vector2 facingDir = Vector2.right;
    private float initialGravityScale;

    public bool flippable = true;
#endregion

    private void Awake()
    {
        rb = Player.Instance.Rb;
        input = GetComponent<PlayerInputHandler>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        dropDownPlatform = GetComponent<PlayerDropDownPlatform>();
        state = Player.State;
        dashEffectController = Player.DashEffect;

        initialGravityScale = rb.gravityScale;

        SetSlideMovement();
    }

    private void Update()
    {
        if (Player.State.IsDead) return;
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
    }

#region FixedUpdate
    private void FixedUpdate()
    {
        bool groundedThisFrame = IsGrounded();

        if (ShouldJump(groundedThisFrame))
        {
            StartJump();
            return;
        }

        if (isJumpProtected ||
            (dropDownPlatform != null && dropDownPlatform.HasStandingPlatform))
        {
            UseLinearVelocityMovement();
            return;
        }

        if (groundedThisFrame)
        {
            UseSlideMovement();
        }
        else
        {
            UseLinearVelocityMovement();
        }
    }
#endregion

#region 지상/경사면
    private void UseSlideMovement()
    {
        SetGravityScale(0f);

        float horizontalInput = input.MoveInput.x;
        if (Mathf.Abs(horizontalInput) < 0.01f)
        {
            horizontalInput = 0f;
        }

        Vector2 groundNormal = GetGroundNormal();
        Vector2 groundTangent = GetRightFacingTangent(groundNormal);

        float movementSpeed = isDashing ? dashSpeed : moveSpeed;
        if (state.IsAttackingGrab || state.IsAttackingSweep)
        {
            movementSpeed = moveSpeedWhileAttack;
        }

        Vector2 slideVelocity = horizontalInput * movementSpeed * groundTangent;

        rb.Slide(slideVelocity, Time.fixedDeltaTime, slideMovement);
    }
#endregion

#region 공중/플랫폼
    private void UseLinearVelocityMovement()
    {
        if (isDashing)
        {
            SetGravityScale(0f);
        }
        else
        {
            SetGravityScale(initialGravityScale);
        }

        float movementSpeed = isDashing ? dashSpeed : moveSpeed;
        if (state.IsAttackingGrab || state.IsAttackingSweep)
        {
            movementSpeed = moveSpeedWhileAttack;
        }

        rb.linearVelocityX = input.MoveInput.x * movementSpeed;
    }
#endregion

#region 점프
    private void ExecuteJump()
    {   
        jumpRequested = true;
    }

    private bool ShouldJump(bool groundedThisFrame)
    {
        bool shouldTryJump = jumpRequested; 
        jumpRequested = false;

        return shouldTryJump && groundedThisFrame && !isJumpProtected && !isDashing;
    }

    private void StartJump()
    {
        isJumpProtected = true;

        SetGravityScale(initialGravityScale);
        rb.linearVelocityY = 0f;
        rb.AddForceY(jumpForce, ForceMode2D.Impulse);

        StartCoroutine(RestoreJump());
    }

    private IEnumerator RestoreJump()
    {
        yield return new WaitForSeconds(jumpProtectionDuration);

        while (!IsGrounded())
        {
            yield return new WaitForFixedUpdate();
        }

        isJumpProtected = false;
    }
#endregion

#region 대시
    private void ExecuteDash()
    {
        if (canDash && input.MoveInput.x != 0)
        {
            StartCoroutine(DashRoutine());
        }
    }
    private IEnumerator DashRoutine()
    {
        canDash = false;
        state.IsDashing = true;
        isDashing = true;
        rb.linearVelocityY = 0f;
        dashEffectController.StartDashTrail();

        yield return new WaitForSeconds(dashTime);

        state.IsDashing = false;
        isDashing = false;
        dashEffectController.StopDashTrail();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
#endregion
    
#region 지면판정 관련
    private bool IsGrounded()
    {
        if (checkGroundedByRay)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                groundCheckPos.position,
                Vector2.down,
                groundCheckRayLength,
                groundLayer
            );

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger)
                {
                    continue;
                }

                bool isIgnoredPlatform = playerBodyCollider != null &&
                    Physics2D.GetIgnoreCollision(
                        playerBodyCollider,
                        hit.collider
                    );

                if (!isIgnoredPlatform)
                {
                    return true;
                }
            }

            return false;
        }

        if (Physics2D.OverlapCircle(
            groundCheckPos.position, groundCheckRayLength, groundLayer))
        {
            return true;
        }
        Debug.Log("접지 떨어짐!");
        return false;
    }
    
    /// <summary>
    /// 아래쪽 땅의 각도를 체크함
    /// </summary>
    private Vector2 GetGroundNormal()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPos.position, Vector2.down, groundCheckRayLength, groundLayer); 
        if (hit.collider != null)
        {
            return hit.normal; 
        }
        
        return Vector2.up;
    }
#endregion

#region 헬퍼함수들
    private static Vector2 GetRightFacingTangent(Vector2 groundNormal)
    {
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x);
        if (tangent.x < 0f)
        {
            tangent = -tangent;
        }

        return tangent.normalized;
    }


    private void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    private void FlipPlayer()
    {
        if (!flippable) return;

        if (Direction2D.IsOpposite(input.MoveInput, facingDir)) Flip();
    }

    private void Flip()
    {
        facingDir = -facingDir;
        transform.Rotate(0f, 180f, 0f);
    }

    public void Animevent_playWalkSound()
    {
    }

    private void SetSlideMovement()
    {
        slideMovement = new Rigidbody2D.SlideMovement
        {
            maxIterations = 4,
            surfaceSlideAngle = 80f,
            gravitySlipAngle = 80f,

            surfaceUp = Vector2.up,
            surfaceAnchor = Vector2.down * 1f,
            gravity = Vector2.up, // 임시방편임. 나중에 고쳐야 할수도 있음!

            selectedCollider = playerBodyCollider,
            useSimulationMove = true,
            useLayerMask = true,
            layerMask = slideMovementLayer
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPos.position, groundCheckRayLength);
    }
#endregion
}

