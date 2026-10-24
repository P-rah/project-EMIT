using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    private Coroutine attackMoveRoutine;

    [Header("Jump Movement")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpProtectionDuration = 0.1f;
    [SerializeField] private bool isJumpProtected;
    [SerializeField] private bool jumpRequested;
    [SerializeField] private float initialGravityScale;
    private float jumpProtectionRemaining;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    // 기존 프리팹의 참조를 보존한다. 접지는 몸체 캡슐의 접촉/캐스트로 판정한다.
    [SerializeField, HideInInspector] private Transform groundCheckPos;
    [Tooltip("접지 중 지면과의 작은 간격을 보정할 최대 캐스트 거리")]
    [SerializeField] private float groundCheckRayLength = 0.3f;
    [InspectorName("Check Grounded")]
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
    public bool CanDash => canDash;

    private Vector2 facingDir = Vector2.right;
    public bool flippable = true;

    private Rigidbody2D rb;
    private CapsuleCollider2D playerBodyCollider;
    private const float MaxGroundAngle = 80f;
    private readonly List<ContactPoint2D> groundContacts = new();
    private readonly List<RaycastHit2D> groundCastHits = new();
    private ContactFilter2D groundFilter;
    private Collider2D groundCollider;
    private Vector2 groundNormal = Vector2.up;
    private float groundSnapDistance;
    private MovingPlatform ride;
    private Vector2 rideDelta;

    private PlayerState state;
    private PlayerInputHandler input;
    private PlayerCombat combat;
    private DashEffectController dashEffectController;
    private PlayerDropDownPlatform dropDownPlatform;
    private PlayerUIController playerUIController;
    private SoundManager soundManager;

    #endregion

    private void Awake()
    {
        rb = Player.Instance.Rb;
        input = GetComponent<PlayerInputHandler>();
        combat = GetComponent<PlayerCombat>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        dropDownPlatform = GetComponent<PlayerDropDownPlatform>();
        state = Player.State;
        dashEffectController = Player.DashEffect;
        playerUIController = Player.UIController;

        initialGravityScale = rb.gravityScale;
        groundFilter = new ContactFilter2D { useTriggers = false };
        groundFilter.SetLayerMask(groundLayer);

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
        else if (!input.UsingInputBuffer && !state.IsAttackingGrab && !state.IsAttackingSweep)
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
        if (attackMoveRoutine != null) StopCoroutine(attackMoveRoutine);
        attackMoveRoutine = StartCoroutine(attackingMoveTimer());
    }

    private IEnumerator attackingMoveTimer()
    {
        triggerMoveWhileAttack = true;
        yield return TimeManager.Instance.WaitForGameplaySeconds(moveTimeWhileAttack);
        triggerMoveWhileAttack = false;
        attackMoveRoutine = null;
    }

#region FixedUpdate
    private void FixedUpdate()
    {
        rideDelta = Vector2.zero;
        jumpProtectionRemaining = Mathf.Max(0f, jumpProtectionRemaining - Time.fixedDeltaTime);

        // 충돌 필터를 먼저 갱신해야 같은 물리 프레임에 플랫폼 지지가 해제된다.
        if (dropDownPlatform != null && dropDownPlatform.isActiveAndEnabled &&
            dropDownPlatform.PreparePhysicsStep(groundCollider))
        {
            ClearGround();
            isJumpProtected = false;
            jumpProtectionRemaining = 0f;
            SetGravityScale(initialGravityScale);
            rb.linearVelocityY = Mathf.Min(rb.linearVelocityY, -dropDownPlatform.DropSpeed);
        }

        bool groundedThisFrame = Grounded();
        if (groundedThisFrame) isJumpProtected = false;

        if (state.IsDead)
        {
            if (groundedThisFrame)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }
            return;
        }

        if (input.UsingInputBuffer)
        {
            // 대시 우선. 실행할 수 없는 대시는 점프의 실행을 막지 않는다.
            if (!input.GameplayInputBlocked && !input.Buffer.TryExecute(Command.Dash) &&
                input.Buffer.TryExecute(Command.Jump)) return;
        }
        else if (ShouldJump(groundedThisFrame))
        {
            StartJump();
            return;
        }

        if (!isJumpProtected && ride != null)
        {
            rideDelta = ride.GetDeltaPositionAndMove();
        }

        // One-way 접촉은 PlatformEffector2D가 처리하도록 물리 속도로 이동한다.
        bool canUseSlide = groundedThisFrame && !isJumpProtected &&
            !PlayerDropDownPlatform.IsOneWayPlatform(groundCollider) &&
            (slideMovementLayer.value & (1 << groundCollider.gameObject.layer)) != 0;
        if (canUseSlide)
        {
            UseSlideMovement();
        }
        else
        {
            UseLinearVelocityMovement(groundedThisFrame && !isJumpProtected);
        }
    }
#endregion

#region 지상/경사면
    private void UseSlideMovement()
    {
        SetGravityScale(0f);

        Vector2 groundTangent = GetRightFacingTangent(groundNormal);
        Vector2 slideVelocity = groundTangent * GetMovementDirection() * GetMovementSpeed();
        slideVelocity += Vector2.down * (groundSnapDistance / Time.fixedDeltaTime);

        // 현재 바닥에서 자체 이동을 계산한 뒤, 플랫폼 이동량과 합쳐 한 번만 적용한다.
        var result = rb.Slide(slideVelocity, Time.fixedDeltaTime, slideMovement);
        rb.MovePosition(result.position + rideDelta);
    }
#endregion

#region 공중/플랫폼
    private void UseLinearVelocityMovement(bool grounded)
    {
        float speed = GetMovementDirection() * GetMovementSpeed();
        if (grounded)
        {
            // 지지면 접선으로 이동하고, 정지 시에는 중력에 의해 미끄러지지 않는다.
            SetGravityScale(0f);
            rb.linearVelocity = GetRightFacingTangent(groundNormal) * speed
                + (rideDelta + Vector2.down * groundSnapDistance) / Time.fixedDeltaTime;
        }
        else
        {
            bool horizontalDash = isDashing && !IsDroppingThroughPlatform;
            SetGravityScale(horizontalDash ? 0f : initialGravityScale);
            rb.linearVelocity = new Vector2(speed, horizontalDash ? 0f : rb.linearVelocity.y);
        }
    }

    private float GetMovementDirection()
    {
        if (isDashing) return facingDir.x;
        return Mathf.Abs(input.MoveInput.x) < 0.01f ? 0f : input.MoveInput.x;
    }

    private float GetMovementSpeed()
    {
        if (isDashing) return dashSpeed;
        if (IsAttacking) return triggerMoveWhileAttack ? moveSpeedWhileAttack : baseMoveSpeedWhileAttack;
        return moveSpeed;
    }

    private bool IsDroppingThroughPlatform => dropDownPlatform != null &&
        dropDownPlatform.isActiveAndEnabled &&
        (dropDownPlatform.IsDropThroughHeld || dropDownPlatform.IsDropping);
#endregion

#region 점프
    private bool IsAttacking => input.UsingInputBuffer && combat != null
        ? combat.IsAttackingGrab || combat.IsAttackingSweep
        : state.IsAttackingGrab || state.IsAttackingSweep;

    public void ClearPendingJumpInput()
    {
        jumpRequested = false;
    }

    // Grounded()가 갱신된 FixedUpdate에서 버퍼 명령이 호출한다.
    public bool TryBufferedJump()
    {
        if (!isActiveAndEnabled || input.GameplayInputBlocked || !IsGrounded ||
            isJumpProtected || isDashing || IsAttacking) return false;

        StartJump();
        IsGrounded = false;
        return true;
    }

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
        jumpProtectionRemaining = jumpProtectionDuration;
        ClearGround();

        SetGravityScale(initialGravityScale);
        rb.linearVelocityY = 0f;
        rb.AddForceY(jumpForce, ForceMode2D.Impulse);
        soundManager.PlaySFX(Player.Instance.jumpSound);
    }
#endregion

#region 대시
    public bool TryBufferedDash()
    {
        if (!isActiveAndEnabled || input.GameplayInputBlocked || !canDash || isDashing)
            return false;
        if (combat != null && combat.IsAttackingGrab) return false;

        bool cancelSweep = combat != null && combat.IsAttackingSweep;
        if (cancelSweep && !combat.TryCancelSweepForBufferedDash()) return false;

        ExecuteDash();
        if (cancelSweep) Player.Animation.StartBufferedDash();
        return true;
    }

    public void ExecuteDash()
    {
        if (canDash)
        {
            StartCoroutine(DashRoutine());
        }
    }
    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;
        if (!IsGrounded && !IsDroppingThroughPlatform) rb.linearVelocityY = 0f;
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
        bool wasGrounded = IsGrounded;
        Collider2D previousGround = groundCollider;
        ClearGround();

        if (!checkGroundedByRay || playerBodyCollider == null || jumpProtectionRemaining > 0f)
            return false;

        float minGroundNormalY = Mathf.Cos(MaxGroundAngle * Mathf.Deg2Rad);
        float skin = Physics2D.defaultContactOffset * 2f;
        playerBodyCollider.GetContacts(groundFilter, groundContacts);
        foreach (ContactPoint2D contact in groundContacts)
        {
            Collider2D surface = contact.collider == playerBodyCollider
                ? contact.otherCollider : contact.collider;
            if (!contact.enabled || !CanStandOn(surface) || contact.normal.y < minGroundNormalY ||
                contact.point.y > playerBodyCollider.bounds.center.y + skin) continue;

            Vector2 surfaceVelocity = surface.attachedRigidbody != null
                ? surface.attachedRigidbody.GetPointVelocity(contact.point) : Vector2.zero;
            if (isJumpProtected && Vector2.Dot(rb.linearVelocity - surfaceVelocity, contact.normal) > 0.01f)
                continue;

            if (!IsGrounded || contact.normal.y > groundNormal.y)
                SetGround(surface, contact.normal);
        }

        if (IsGrounded) return true;

        // 새로운 착지는 활성 접촉으로만 인정한다. 캐스트는 기존 지지를 짧게
        // 이어 주는 데만 사용하여 아래에서 통과 중인 플랫폼에 붙지 않게 한다.
        if (!wasGrounded || isJumpProtected) return false;

        playerBodyCollider.Cast(Vector2.down, groundFilter, groundCastHits, groundCheckRayLength, true);
        float closestDistance = float.PositiveInfinity;
        foreach (RaycastHit2D hit in groundCastHits)
        {
            if (!CanStandOn(hit.collider) || hit.distance <= 0f ||
                hit.normal.y < minGroundNormalY || hit.distance >= closestDistance) continue;
            if (PlayerDropDownPlatform.IsOneWayPlatform(hit.collider) && hit.collider != previousGround)
                continue;

            closestDistance = hit.distance;
            SetGround(hit.collider, hit.normal);
            groundSnapDistance = Mathf.Max(0f, hit.distance - skin);
        }

        return IsGrounded;
    }

    private bool CanStandOn(Collider2D surface)
    {
        return surface != null && !surface.isTrigger && surface.attachedRigidbody != rb &&
            !Physics2D.GetIgnoreLayerCollision(playerBodyCollider.gameObject.layer, surface.gameObject.layer) &&
            !Physics2D.GetIgnoreCollision(playerBodyCollider, surface);
    }

    private void SetGround(Collider2D surface, Vector2 normal)
    {
        groundCollider = surface;
        groundNormal = normal.normalized;
        ride = surface.attachedRigidbody != null
            ? surface.attachedRigidbody.GetComponent<MovingPlatform>() : null;
        if (ride != null && !ride.isActiveAndEnabled) ride = null;
        IsGrounded = true;
    }

    private void ClearGround()
    {
        groundCollider = null;
        groundNormal = Vector2.up;
        groundSnapDistance = 0f;
        ride = null;
        IsGrounded = false;
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
            surfaceSlideAngle = MaxGroundAngle,
            gravitySlipAngle = 90f,

            surfaceUp = Vector2.up,
            surfaceAnchor = Vector2.down * groundCheckRayLength,
            gravity = Vector2.zero, // 접지 속도에 별도의 중력 이동을 더하지 않는다.

            selectedCollider = playerBodyCollider,
            useNoMove = true,
            useSimulationMove = false,
            useLayerMask = true,
            layerMask = slideMovementLayer
        };
    }

#endregion
}

