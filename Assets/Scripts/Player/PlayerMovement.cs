using UnityEngine;
using System.Collections;
/*
 * 플레이어 이동 제어용 클래스
 */
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private PlayerState state;

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float jumpForce = 15f;

    [Header("Dash Settings")]
    public float dashPower = 40f;
    public float dashTime = 0.1f;
    public float dashCooldown = 0.5f;
    private float facingDirection = 1f;
    private float originalGravity;
    private bool canDash = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputHandler>();
        state = GetComponent<PlayerState>();

        originalGravity = rb.gravityScale;
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnJumpPressed", ExecuteJump);
        EventManager.StartListening("OnDashPressed", ExecuteDash);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnJumpPressed", ExecuteJump);
        EventManager.StopListening("OnDashPressed", ExecuteDash);
    }

    private void Update()
    {
        if (input.MoveInput.x > 0 && facingDirection < 0f) Flip();
        else if (input.MoveInput.x < 0 && facingDirection > 0f) Flip();
    }

    private void FixedUpdate()
    {
        if (state.IsDashing) return;
        rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    private void ExecuteJump()
    {
        if (state.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void ExecuteDash()
    {
        if(canDash) StartCoroutine(DashRoutine());
    }

    private void Flip()
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

        yield return new WaitForSeconds(dashTime);

        state.IsDashing = false;
        rb.gravityScale = originalGravity;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
