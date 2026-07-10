using UnityEngine;
using System.Collections;
/*
 * 플레이어 이동 제어용 클래스
 */
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private PlayerState state;
    private DashEffectController dashEffectController;

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float moveSpeedWhileAttack = 5f;
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
        rb = Player.Instance.Rb;
        input = Player.InputHandler;
        state = Player.State;
        dashEffectController = Player.DashEffect;

        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        if (input.MoveInput.x > 0 && facingDirection < 0f) Flip();
        else if (input.MoveInput.x < 0 && facingDirection > 0f) Flip();
    }

    private void FixedUpdate()
    {
        if (state.CurrentState == State.Dashing) return;
        else if (state.CurrentState == State.AttackingSweep || state.CurrentState == State.AttackingGrab)
            rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeedWhileAttack, rb.linearVelocity.y);
        else rb.linearVelocity = new Vector2(input.MoveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    public void ExecuteJump()
    {
        if (state.IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void ExecuteDash()
    {
        if(canDash&& input.MoveInput.x !=0) StartCoroutine(DashRoutine());
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
}
