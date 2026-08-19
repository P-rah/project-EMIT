using UnityEngine;
using System.Collections;
using EMIT.Utilities;

/*
 * 플레이어 이동 제어용 클래스
 */
[DefaultExecutionOrder(100)]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private PlayerState state;
    private DashEffectController dashEffectController;
    private PlayerDropDownPlatform playerDropDownPlatform;

    [Header("Movement Status&Flags")]
    [SerializeField] private bool isOnGround;
    [SerializeField] private bool canJump;
    [SerializeField] private bool isMovingUpwards;
    [SerializeField] private bool isMovingDownwards;
    [SerializeField] private bool isOnSlope;
    [SerializeField] private bool isExitingSlope;
    [SerializeField] private bool isDroppingDownFromPlatform;
    [SerializeField] private bool isDashing;


    [Header("FixedUpdate Method")]
    [SerializeField] private bool usePrevious = false;

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float moveSpeedWhileAttack = 0f;

    
    [Header("Jump Movement")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private BoxCollider2D edgeGuard;
    [SerializeField] private float jumpCoyoteTime = 0f; 

    [Header("Slope Movement")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRayLength;

    [Header("Platform Movement")]
    private MovingPlatform currentGroundDeltaProvider;
    private Vector2 currentGroundDeltaPosition;
    private Vector2 lastGroundDeltaVelocity;
    private Vector2 airborneInheritedGroundDeltaVelocity;

    private const int GroundQueryBufferSize = 16;
    private const float MinimumGroundNormalY = 0.17364818f;
    private const float GroundNormalComparisonEpsilon = 0.0001f;
    private const float FootContactTolerance = 0.02f;

    private readonly Collider2D[] groundColliderBuffer =
        new Collider2D[GroundQueryBufferSize];
    private readonly ContactPoint2D[] groundContactBuffer =
        new ContactPoint2D[GroundQueryBufferSize];

    private ContactFilter2D groundOverlapFilter;
    private ContactFilter2D groundCollisionFilter;
    private CapsuleCollider2D playerBodyCollider;
    private Collider2D currentGroundCollider;
    private Rigidbody2D currentGroundRigidbody;
    private float coyoteTimeRemaining;
    private bool jumpRequested;
    private bool wasGrounded;

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
        edgeGuard = GetComponent<BoxCollider2D>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        state = Player.State;
        dashEffectController = Player.DashEffect;

        initialGravityScale = rb.gravityScale;

        groundOverlapFilter = new ContactFilter2D();
        groundOverlapFilter.SetLayerMask(groundLayer);
        groundOverlapFilter.useTriggers = true;

        groundCollisionFilter = new ContactFilter2D();
        groundCollisionFilter.SetLayerMask(groundLayer);
        groundCollisionFilter.useTriggers = false;
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
        if (input.ConsumeDown())
        {
            ExecuteDown();
        }
    }

    private void FixedUpdate()
    {
        if (usePrevious)
        {
            PreviousFixedUpdate();
            return;
        }

        RefactoredFixedUpdate();
    }

    private void PreviousFixedUpdate()
    {
        if (Player.State.IsDead) return;
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

        // 
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
        if (!usePrevious)
        {
            jumpRequested = true;
            return;
        }

        if (!IsGrounded())
        {
            return;
        }
        isExitingSlope = true;
        rb.gravityScale = initialGravityScale;
        rb.linearVelocityY = 0f;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        // AudioManager.Instance.PlaySFX(Player.Instance.jumpSound);
        SoundManager.instance.PlaySFX(Player.Instance.jumpSound);
    }

    private void RefactoredFixedUpdate()
    {
        if (Player.State.IsDead)
        {
            jumpRequested = false;
            currentGroundDeltaPosition = Vector2.zero;
            lastGroundDeltaVelocity = Vector2.zero;
            airborneInheritedGroundDeltaVelocity = Vector2.zero;
            SetCurrentGround(null);
            return;
        }

        isDroppingDownFromPlatform =
            playerDropDownPlatform != null && playerDropDownPlatform.IsDropping;
        isDashing = state.IsDashing;

        if (isDroppingDownFromPlatform)
        {
            HandleRefactoredDropDown();
            return;
        }

        if (isDashing)
        {
            jumpRequested = false;
            coyoteTimeRemaining = 0f;
            canJump = false;
            isOnGround = false;
            isOnSlope = false;
            wasGrounded = false;
            currentGroundDeltaPosition = Vector2.zero;
            lastGroundDeltaVelocity = Vector2.zero;
            airborneInheritedGroundDeltaVelocity = Vector2.zero;
            SetCurrentGround(null);
            return;
        }

        GroundSupport support;
        bool hasSupport = TryGetGroundSupport(out support);

        if (isExitingSlope &&
            rb.linearVelocityY <=
                lastGroundDeltaVelocity.y + GroundNormalComparisonEpsilon)
        {
            isExitingSlope = false;
        }

        bool grounded = hasSupport && !isExitingSlope;
        isOnGround = grounded;
        isOnSlope = grounded &&
            Vector2.Angle(Vector2.up, support.Normal) > 0.05f;

        if (grounded)
        {
            SetCurrentGround(support.Collider);
            currentGroundDeltaPosition = GetCurrentGroundDeltaPosition();
            lastGroundDeltaVelocity = DeltaPositionToVelocity(
                currentGroundDeltaPosition
            );
            airborneInheritedGroundDeltaVelocity = lastGroundDeltaVelocity;
            coyoteTimeRemaining = Mathf.Max(0f, jumpCoyoteTime);
        }
        else
        {
            SetCurrentGround(null);
            currentGroundDeltaPosition = Vector2.zero;
            coyoteTimeRemaining = Mathf.Max(
                0f,
                coyoteTimeRemaining - Time.fixedDeltaTime
            );
        }

        canJump = grounded || coyoteTimeRemaining > 0f;

        bool shouldTryJump = jumpRequested;
        jumpRequested = false;
        if (shouldTryJump && canJump)
        {
            ExecuteRefactoredJump(grounded, support.Normal);
            return;
        }

        float controlledMoveSpeed = GetControlledMoveSpeed();
        if (grounded)
        {
            Vector2 groundTangent = GetRightFacingTangent(support.Normal);
            Vector2 playerDeltaPosition = groundTangent *
                (input.MoveInput.x * controlledMoveSpeed * Time.fixedDeltaTime);
            Vector2 totalDeltaPosition =
                currentGroundDeltaPosition + playerDeltaPosition;

            SetGravityScale(0f);
            rb.MovePosition(rb.position + totalDeltaPosition);
            wasGrounded = true;
            UpdateVerticalMovementFlagsFromDelta(totalDeltaPosition);
        }
        else
        {
            if (wasGrounded && !isExitingSlope)
            {
                rb.linearVelocityY = lastGroundDeltaVelocity.y;
            }

            SetGravityScale(initialGravityScale);
            rb.linearVelocityX = airborneInheritedGroundDeltaVelocity.x +
                input.MoveInput.x * controlledMoveSpeed;
            wasGrounded = false;
            UpdateVerticalMovementFlags();
        }
    }

    private void HandleRefactoredDropDown()
    {
        Vector2 latestGroundDelta = GetCurrentGroundDeltaPosition();
        if (currentGroundDeltaProvider != null)
        {
            airborneInheritedGroundDeltaVelocity =
                DeltaPositionToVelocity(latestGroundDelta);
        }

        jumpRequested = false;
        coyoteTimeRemaining = 0f;
        canJump = false;
        isOnGround = false;
        isOnSlope = false;
        isExitingSlope = false;
        wasGrounded = false;
        currentGroundDeltaPosition = Vector2.zero;
        SetCurrentGround(null);

        SetGravityScale(initialGravityScale);
        rb.linearVelocityY = Mathf.Min(
            rb.linearVelocityY,
            airborneInheritedGroundDeltaVelocity.y
        );
        rb.linearVelocityX = airborneInheritedGroundDeltaVelocity.x +
            input.MoveInput.x * GetControlledMoveSpeed();
        UpdateVerticalMovementFlags();
    }

    private void ExecuteRefactoredJump(
        bool jumpedFromSupport,
        Vector2 supportNormal)
    {
        Vector2 takeoffVelocity = rb.linearVelocity;
        if (jumpedFromSupport)
        {
            Vector2 groundTangent = GetRightFacingTangent(supportNormal);
            Vector2 playerVelocity = groundTangent *
                (input.MoveInput.x * GetControlledMoveSpeed());
            takeoffVelocity = lastGroundDeltaVelocity + playerVelocity;
        }
        else
        {
            takeoffVelocity.x =
                airborneInheritedGroundDeltaVelocity.x +
                input.MoveInput.x * GetControlledMoveSpeed();
        }

        // 플레이어의 경사 접선 Y는 제거하고, 지면 Delta의 Y만 관성으로 전환한다.
        takeoffVelocity.y = lastGroundDeltaVelocity.y;
        rb.linearVelocity = takeoffVelocity;
        SetGravityScale(initialGravityScale);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpRequested = false;
        coyoteTimeRemaining = 0f;
        canJump = false;
        isOnGround = false;
        isOnSlope = false;
        isExitingSlope = true;
        wasGrounded = false;
        currentGroundDeltaPosition = Vector2.zero;
        SetCurrentGround(null);
        UpdateVerticalMovementFlags();

        // AudioManager.Instance.PlaySFX(Player.Instance.jumpSound);
        SoundManager.instance.PlaySFX(Player.Instance.jumpSound);
    }

    private Vector2 GetCurrentGroundDeltaPosition()
    {
        if (currentGroundDeltaProvider == null)
        {
            return Vector2.zero;
        }

        return currentGroundDeltaProvider
            .GetDeltaPositionAndMove();
    }

    private static Vector2 DeltaPositionToVelocity(Vector2 deltaPosition)
    {
        if (Time.fixedDeltaTime <= Mathf.Epsilon)
        {
            return Vector2.zero;
        }

        return deltaPosition / Time.fixedDeltaTime;
    }

    private void UpdateVerticalMovementFlagsFromDelta(Vector2 deltaPosition)
    {
        isMovingUpwards =
            deltaPosition.y > GroundNormalComparisonEpsilon;
        isMovingDownwards =
            deltaPosition.y < -GroundNormalComparisonEpsilon;
    }

    private bool TryGetGroundSupport(out GroundSupport bestSupport)
    {
        bestSupport = default;
        bool foundSupport = false;

        int contactCount = rb.GetContacts(
            groundCollisionFilter,
            groundContactBuffer
        );

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = groundContactBuffer[i];
            Collider2D groundCollider = GetGroundCollider(contact);
            if (groundCollider == null || !IsFootContactPoint(contact.point))
            {
                continue;
            }

            Vector2 groundNormal = GetNormalTowardPlayer(
                contact.normal,
                contact.point,
                rb.worldCenterOfMass
            );
            TrySelectGroundSupport(
                new GroundSupport(
                    groundCollider,
                    groundNormal,
                    contact.point
                ),
                ref foundSupport,
                ref bestSupport
            );
        }

        if (edgeGuard == null)
        {
            return foundSupport;
        }

        if (!edgeGuard.enabled)
        {
            edgeGuard.enabled = true;
        }

        int overlapCount = edgeGuard.Overlap(
            groundOverlapFilter,
            groundColliderBuffer
        );

        for (int i = 0; i < overlapCount; i++)
        {
            Collider2D groundCollider = groundColliderBuffer[i];
            if (!IsValidGroundCollider(groundCollider))
            {
                continue;
            }

            ColliderDistance2D distance = edgeGuard.Distance(groundCollider);
            if (!distance.isValid)
            {
                continue;
            }

            Vector2 groundNormal = GetNormalTowardPlayer(
                distance.normal,
                distance.pointB,
                edgeGuard.bounds.center
            );
            TrySelectGroundSupport(
                new GroundSupport(
                    groundCollider,
                    groundNormal,
                    distance.pointB
                ),
                ref foundSupport,
                ref bestSupport
            );
        }

        return foundSupport;
    }

    private Collider2D GetGroundCollider(ContactPoint2D contact)
    {
        if (IsValidGroundCollider(contact.collider) &&
            contact.collider.attachedRigidbody != rb)
        {
            return contact.collider;
        }

        if (IsValidGroundCollider(contact.otherCollider) &&
            contact.otherCollider.attachedRigidbody != rb)
        {
            return contact.otherCollider;
        }

        return null;
    }

    private bool IsValidGroundCollider(Collider2D candidate)
    {
        if (candidate == null || candidate.isTrigger)
        {
            return false;
        }

        if (Physics2D.GetIgnoreLayerCollision(
            gameObject.layer,
            candidate.gameObject.layer))
        {
            return false;
        }

        if (playerBodyCollider != null &&
            Physics2D.GetIgnoreCollision(playerBodyCollider, candidate))
        {
            return false;
        }

        int candidateLayerMask = 1 << candidate.gameObject.layer;
        return (groundLayer.value & candidateLayerMask) != 0;
    }

    private bool IsFootContactPoint(Vector2 contactPoint)
    {
        if (edgeGuard != null)
        {
            Bounds footBounds = edgeGuard.bounds;
            return contactPoint.x >= footBounds.min.x - FootContactTolerance &&
                contactPoint.x <= footBounds.max.x + FootContactTolerance &&
                contactPoint.y <= footBounds.max.y + FootContactTolerance;
        }

        if (groundCheckPos != null)
        {
            return contactPoint.y <=
                groundCheckPos.position.y + FootContactTolerance;
        }

        return contactPoint.y <= rb.worldCenterOfMass.y;
    }

    private void TrySelectGroundSupport(
        GroundSupport candidate,
        ref bool foundSupport,
        ref GroundSupport bestSupport)
    {
        if (candidate.Normal.y <= MinimumGroundNormalY)
        {
            return;
        }

        if (!foundSupport)
        {
            bestSupport = candidate;
            foundSupport = true;
            return;
        }

        float normalDifference = candidate.Normal.y - bestSupport.Normal.y;
        if (normalDifference > GroundNormalComparisonEpsilon)
        {
            bestSupport = candidate;
            return;
        }

        if (Mathf.Abs(normalDifference) > GroundNormalComparisonEpsilon)
        {
            return;
        }

        bool candidateIsCurrentGround =
            candidate.Collider == currentGroundCollider;
        bool bestIsCurrentGround =
            bestSupport.Collider == currentGroundCollider;

        if (candidateIsCurrentGround && !bestIsCurrentGround)
        {
            bestSupport = candidate;
            return;
        }

        if (candidateIsCurrentGround == bestIsCurrentGround &&
            candidate.Point.y > bestSupport.Point.y)
        {
            bestSupport = candidate;
        }
    }

    private static Vector2 GetNormalTowardPlayer(
        Vector2 normal,
        Vector2 surfacePoint,
        Vector2 playerPoint)
    {
        if (normal.sqrMagnitude < Mathf.Epsilon)
        {
            return Vector2.zero;
        }

        Vector2 surfaceToPlayer = playerPoint - surfacePoint;
        if (surfaceToPlayer.sqrMagnitude >= Mathf.Epsilon &&
            Vector2.Dot(normal, surfaceToPlayer) < 0f)
        {
            normal = -normal;
        }

        return normal.normalized;
    }

    private static Vector2 GetRightFacingTangent(Vector2 groundNormal)
    {
        Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x);
        if (tangent.x < 0f)
        {
            tangent = -tangent;
        }

        return tangent.normalized;
    }

    private float GetControlledMoveSpeed()
    {
        if (state.IsAttackingSweep || state.IsAttackingGrab)
        {
            return moveSpeedWhileAttack;
        }

        return moveSpeed;
    }

    private void SetCurrentGround(Collider2D groundCollider)
    {
        if (currentGroundCollider == groundCollider)
        {
            return;
        }

        currentGroundCollider = groundCollider;
        Rigidbody2D nextGroundRigidbody = currentGroundCollider == null
            ? null
            : currentGroundCollider.attachedRigidbody;
        if (currentGroundRigidbody == nextGroundRigidbody)
        {
            return;
        }

        currentGroundRigidbody = nextGroundRigidbody;
        currentGroundDeltaProvider = currentGroundRigidbody == null
            ? null
            : currentGroundRigidbody.GetComponent<MovingPlatform>();
    }

    private void UpdateVerticalMovementFlags()
    {
        isMovingUpwards = rb.linearVelocityY > GroundNormalComparisonEpsilon;
        isMovingDownwards = rb.linearVelocityY < -GroundNormalComparisonEpsilon;
    }

    private struct GroundSupport
    {
        public Collider2D Collider;
        public Vector2 Normal;
        public Vector2 Point;

        public GroundSupport(
            Collider2D collider,
            Vector2 normal,
            Vector2 point)
        {
            Collider = collider;
            Normal = normal;
            Point = point;
        }
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
        // AudioManager.Instance.PlaySFX(Player.Instance.dashSound);
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

    public void StopMove()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = initialGravityScale;
        if (!usePrevious)
        {
            currentGroundDeltaPosition = Vector2.zero;
            lastGroundDeltaVelocity = Vector2.zero;
            airborneInheritedGroundDeltaVelocity = Vector2.zero;
            SetCurrentGround(null);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!usePrevious)
        {
            return;
        }

        Debug.Log("Edge Guard Enabled");
        edgeGuard.enabled = true;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!usePrevious)
        {
            return;
        }

        Debug.Log("Edge Guard Disabled");
        edgeGuard.enabled = false;

    }

    public void Animevent_playWalkSound()
    {
        // AudioManager.Instance.PlaySFX(Player.Instance.walkSound, 0.2f);
        SoundManager.instance.PlaySFX(Player.Instance.walkSound, 0.2f);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPos.position, 0.2f);
    }
}
