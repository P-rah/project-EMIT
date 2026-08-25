    using UnityEngine;
/*
 * 플레이어 애니메이션 관리용 클래스
 */
public class PlayerAnimation : MonoBehaviour
{
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
        AnimationControl();
        previousState = state.CurrentState;
    }

    private void Update()
    {

        if (previousState != state.CurrentState)
        {
            AnimationControl();
            previousState = state.CurrentState;
        }
    }

    private void AnimationControl()
    {
        switch(state.CurrentState)
        {
            case State.Idle:
                animator.SetBool("isJump", false);
                animator.SetBool("isRun",false);
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
                break;
            case State.AttackingGrab:
                animator.SetTrigger("isGrap");
                break;
            case State.Dashing:
                animator.SetBool("isDash", true);
                break;
            case State.Dead:
                animator.SetBool("isDeath", true);
                break;

        }
    }
    public void SetAttackSweep(int currentCombo)
    {
        animator.SetTrigger("isAttack");
        animator.SetInteger("sweepCombo", currentCombo);
        Debug.Log("attackCount : " + currentCombo);
    }
    public void ResetAttackSweep()
    {
        animator.SetInteger("sweepCombo", 0);
        Debug.Log("attackCount리셋");
    }
}
