using UnityEngine;
/*
 * 플레이어 애니메이션 관리용 클래스
 */
[RequireComponent(typeof(PlayerState))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerState state;
    private State previousState;
    private bool attacked = false;
    private bool attackReverse = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<PlayerState>();
    }

    private void OnEnable()
    {
        previousState = state.CurrentState;
        AnimationControll();
    }

    private void Update()
    {
        if (attacked != state.attacked)
        {
            if (state.attacked)
            {
                animator.SetBool("attackReverse", !attackReverse);
            }
            else
            {
                animator.SetTrigger("noMoreAttack");
                attackReverse = false;
            }
            attacked = state.attacked;
        }

        if (previousState != state.CurrentState)
        {
            AnimationControll();
            previousState = state.CurrentState;
        }
    }

    private void AnimationControll()
    {
        switch(state.CurrentState)
        {
            case State.Idle:
                animator.SetBool("isRun",false);
                animator.SetBool("isGround", true);
                animator.SetBool("isDash", false);
                break;
            case State.Running:
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
            case State.AttackingGrap:
                animator.SetBool("attackGrap", true);
                animator.SetTrigger("isAttack");
                break;
            case State.Dashing:
                animator.SetBool("isDash", true);
                break;
        }
    }
}
