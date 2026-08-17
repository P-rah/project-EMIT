using UnityEngine;

/*
 * 플레이어 애니메이션 관리용 클래스
 */
public class PlayerAnimation : MonoBehaviour
{
    private const string ShortSweepStateName = "Base Layer.Deva_sweep1";
    private const string LongSweepStateName = "Base Layer.Deva_sweep1";
    private const string GrabStateName = "Base Layer.Deva_grap1";

    private Animator animator;
    private PlayerState state;
    private State previousState;

    private void Awake()
    {
        animator = Player.Instance.animator;
        state = Player.State;
    }

    private void OnEnable()
    {
        UpdateLocomotionAnimation();
        previousState = state.CurrentState;
    }

    private void Update()
    {
        if (previousState == state.CurrentState)
            return;

        if (!IsAttackState(state.CurrentState))
        {
            UpdateLocomotionAnimation();
        }

        previousState = state.CurrentState;
    }

    public void PlayShortSweep()
    {
        PlayAttack(ShortSweepStateName);
    }

    public void PlayLongSweep()
    {
        // 현재는 Short Sweep과 같은 상태를 사용한다.
        // 추후 Long Sweep 애니메이션이 추가되면 LongSweepStateName만 변경하면 된다.
        PlayAttack(LongSweepStateName);
    }

    public void PlayGrab()
    {
        PlayAttack(GrabStateName);
    }

    private void PlayAttack(string stateName)
    {
        animator.Play(stateName, 0, 0f);
    }

    private void UpdateLocomotionAnimation()
    {
        switch (state.CurrentState)
        {
            case State.Idle:
                animator.SetBool("isJump", false);
                animator.SetBool("isRun", false);
                animator.SetBool("isGround", true);
                animator.SetBool("isDash", false);
                break;
            case State.Running:
                animator.SetBool("isJump", false);
                animator.SetBool("isRun", true);
                animator.SetBool("isGround", true);
                animator.SetBool("isDash", false);
                break;
            case State.Jumping:
                animator.SetBool("isJump", true);
                animator.SetBool("isGround", false);
                animator.SetBool("isDash", false);
                break;
            case State.Falling:
                animator.SetBool("isJump", false);
                animator.SetBool("isGround", false);
                animator.SetBool("isDash", false);
                break;
            case State.Dashing:
                animator.SetBool("isDash", true);
                break;
            case State.Death:
                animator.SetBool("isDeath", true);
                break;
        }
    }

    private static bool IsAttackState(State currentState)
    {
        return currentState == State.AttackingSweep || currentState == State.AttackingGrab;
    }

    // Legacy state-driven animation path. 연결은 제거했으며 롤백 참고용으로 유지한다.
    private void LegacyStateDrivenAnimationUpdate()
    {
        if (previousState != state.CurrentState)
        {
            LegacyAnimationControl();
            previousState = state.CurrentState;
        }
    }

    private void LegacyAnimationControl()
    {
        switch (state.CurrentState)
        {
            case State.Idle:
                animator.SetBool("isJump", false);
                animator.SetBool("isRun", false);
                animator.SetBool("isGround", true);
                animator.SetBool("isDash", false);
                break;
            case State.Running:
                animator.SetBool("isJump", false);
                animator.SetBool("isRun", true);
                animator.SetBool("isGround", true);
                animator.SetBool("isDash", false);
                break;
            case State.Jumping:
                animator.SetBool("isJump", true);
                animator.SetBool("isGround", false);
                animator.SetBool("isDash", false);
                break;
            case State.Falling:
                animator.SetBool("isJump", false);
                animator.SetBool("isGround", false);
                animator.SetBool("isDash", false);
                break;
            case State.AttackingSweep:
                animator.SetBool("attackGrap", false);
                animator.SetTrigger("isAttack");
                break;
            case State.AttackingGrab:
                animator.SetBool("attackGrap", true);
                animator.SetTrigger("isAttack");
                break;
            case State.Dashing:
                animator.SetBool("isDash", true);
                break;
            case State.Death:
                animator.SetBool("isDeath", true);
                break;
        }
    }
}
