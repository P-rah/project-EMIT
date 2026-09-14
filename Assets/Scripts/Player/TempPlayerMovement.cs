using UnityEngine;
using System.Collections;
using EMIT.Utilities;

public class TempPlayerMovement : MonoBehaviour
{
#region 변수 선언

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float baseMoveSpeedWhileAttack = 0f;
    [SerializeField] private float moveSpeedWhileAttack = 5f;
    [SerializeField] private float moveTimeWhileAttack = 0.2f;
    [SerializeField] private bool triggerMoveWhileAttack = false;

    [Header("Jump Movement")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpProtectionDuration = 0.1f;
    [SerializeField] private bool isJumpProtected;
    [SerializeField] private bool jumpRequested;
    [SerializeField] private float initialGravityScale;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRayLength = 0.3f;
    [SerializeField] private bool checkGroundedByRay = true;
    public bool IsGrounded { get; private set; }

    [Header("Slope Movement")]
    [SerializeField] private LayerMask slideMovementLayer;
    [SerializeField] private Rigidbody2D.SlideMovement slideMovement;

    [Header("Dash Settings")]
    [SerializeField] private float dashTime = 0.1f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashSpeed = 40f;
    [SerializeField] private bool canDash = true;
    [SerializeField] private bool isDashing;
    public bool IsDashing => isDashing;

    private Vector2 facingDir = Vector2.right;
    public bool flippable = true;

    private Rigidbody2D rb;
    private CapsuleCollider2D playerBodyCollider;
    private RaycastHit2D groundHit;
    private MovingPlatform ride;
    private Vector2 rideDelta;

    private PlayerState state;
    private PlayerInputHandler input;
    private DashEffectController dashEffectController;
    private PlayerDropDownPlatform dropDownPlatform;
    private PlayerUIController playerUIController;
    private SoundManager soundManager;

    #endregion

    private void Awake()
    {
        rb = Player.Instance.Rb;
        input = GetComponent<PlayerInputHandler>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        dropDownPlatform = GetComponent<PlayerDropDownPlatform>();
        state = Player.State;
        dashEffectController = Player.DashEffect;
        playerUIController = Player.UIController;

        initialGravityScale = rb.gravityScale;

        SetSlideMovement();
    }

    private void Start()
    {
        soundManager = SoundManager.Instance;
    }

    private void Update()
    {
        if (state.IsDead)
        {
            return;
        }
        else if (!state.IsAttackingGrab && !state.IsAttackingSweep)
        {
            HandleInputs();
        }
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

    public void TriggerMoveWhileAttack()
    {
        StopCoroutine(attackingMoveTimer());
        StartCoroutine(attackingMoveTimer());
    }

    private IEnumerator attackingMoveTimer()
    {
        triggerMoveWhileAttack = true;
        yield return new WaitForSecondsRealtime(moveTimeWhileAttack);
        triggerMoveWhileAttack = false;
    }

#region FixedUpdate
    private void FixedUpdate()
    {
        rideDelta = Vector2.zero;
        bool groundedThisFrame = Grounded();

        if (state.IsDead)
        {
            if (groundedThisFrame)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }
            return;
        }

        if (ShouldJump(groundedThisFrame))
        {
            StartJump();
            return;
        }

        if (!isJumpProtected && ride != null)
        {
            rideDelta = ride.GetDeltaPositionAndMove();
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
            if (triggerMoveWhileAttack)
            {
                movementSpeed = moveSpeedWhileAttack;
            }
            else movementSpeed = baseMoveSpeedWhileAttack;
        }

        Vector2 slideVelocity = horizontalInput * movementSpeed * groundTangent;

        // 현재 바닥에서 자체 이동을 계산한 뒤, 플랫폼 이동량과 합쳐 한 번만 적용한다.
        var result = rb.Slide(slideVelocity, Time.fixedDeltaTime, slideMovement);
        rb.MovePosition(result.position + rideDelta);
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
            if (triggerMoveWhileAttack) movementSpeed = moveSpeedWhileAttack;
            else movementSpeed = baseMoveSpeedWhileAttack;
        }

        if (!isJumpProtected && ride != null)
        {
            // 단방향 발판에서는 기존 속도 이동을 유지하고 수직 운행도 따라간다.
            SetGravityScale(0f);
            rb.linearVelocity = new Vector2(input.MoveInput.x * movementSpeed, 0f)
                + rideDelta / Time.fixedDeltaTime;
        }
        else
        {
            rb.linearVelocityX = input.MoveInput.x * movementSpeed;
        }
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
        soundManager.PlaySFX(Player.Instance.jumpSound);

        StartCoroutine(RestoreJump());
    }

    private IEnumerator RestoreJump()
    {
        yield return new WaitForSeconds(jumpProtectionDuration);

        while (!Grounded())
        {
            yield return new WaitForFixedUpdate();
        }

        isJumpProtected = false;
    }
#endregion

#region 대시
    public void ExecuteDash()
    {
        if (canDash && input.MoveInput.x != 0)
        {
            StartCoroutine(DashRoutine());
        }
    }
    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        rb.linearVelocityY = 0f;
        dashEffectController.StartDashTrail();
        soundManager.PlaySFX(Player.Instance.dashSound);

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        dashEffectController.StopDashTrail();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
#endregion

    
#region 지면판정 관련
    private bool Grounded()
    {
        groundHit = default;
        ride = null;
        IsGrounded = false;
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
                if (hit.collider == null || hit.collider.isTrigger ||
                    hit.rigidbody == rb || hit.normal.y <= 0f)
                {
                    continue;
                }

                bool isIgnoredPlatform = playerBodyCollider != null &&
                    (Physics2D.GetIgnoreLayerCollision(
                        playerBodyCollider.gameObject.layer, hit.collider.gameObject.layer) ||
                    Physics2D.GetIgnoreCollision(
                        playerBodyCollider,
                        hit.collider
                    ));

                if (!isIgnoredPlatform)
                {
                    groundHit = hit;
                    ride = hit.rigidbody != null
                        ? hit.rigidbody.GetComponent<MovingPlatform>()
                        : null;
                    if (ride != null && !ride.isActiveAndEnabled)
                    {
                        ride = null;
                    }
                    IsGrounded = true;
                    return true;
                }
            }
            IsGrounded = false;
            return false;
        }
        Debug.Log("접지 떨어짐!");
        return false;
    }
    
    /// <summary>
    /// 아래쪽 땅의 각도를 체크함
    /// </summary>
    private Vector2 GetGroundNormal()
    {
        return groundHit.collider != null ? groundHit.normal : Vector2.up;
    }
#endregion

#region 움직임 정지 API

    public void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = initialGravityScale;
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
        //playerUIController.SetUIFlip();
    }

    public void Animevent_PlayWalkSound()
    {
        soundManager.PlaySFX(Player.Instance.walkSound, 0.2f);
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
            useNoMove = true,
            useSimulationMove = false,
            useLayerMask = true,
            layerMask = slideMovementLayer
        };
    }

#endregion
}

