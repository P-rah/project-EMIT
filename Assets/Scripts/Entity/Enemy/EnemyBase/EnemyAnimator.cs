using EMIT.Utilities;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private GameObject attackEffectObject;

    private readonly int hashIdle = Animator.StringToHash("EnemyIdle");
    private readonly int hashWalk = Animator.StringToHash("EnemyWalk");
    private readonly int hashHitFront = Animator.StringToHash("EnemyHitFront");
    private readonly int hashHitBack = Animator.StringToHash("EnemyHitBack");
    private readonly int hashAttack = Animator.StringToHash("EnemyAttack");
    private readonly int hashAttackEnd = Animator.StringToHash("EnemyAttackEnd");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        StartAnimControl();
    }

    public void StopAnimControl()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public void StartAnimControl()
    {
        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    internal void HandleStateEntered(Enemy owner,StateBase<EnemyStateMachine> state)
    {
        if (owner == null || state == null || owner.StateMachine == null)
        {
            return;
        }

        EnemyStateMachine stateMachine = owner.StateMachine;

        switch (state)
        {
            case var _ when state == stateMachine.IdleState:
                PlayIdle();
                break;

            case var _ when state == stateMachine.ChaseState:
                PlayWalk();
                break;

            case var _ when state == stateMachine.KnockbackState:
                if (Direction2D.IsOpposite(owner.currentKnockBackForce, owner.FacingDir))
                {
                    PlayHitFront();
                }
                else
                {
                    PlayHitBack();
                }
                break;
            case var _ when state == stateMachine.GrabbedState:
                if (Direction2D.IsOpposite(owner.currentKnockBackForce, owner.FacingDir))
                {
                    PlayHitBack();
                }
                else
                {
                    PlayHitFront();
                }
                break;
        }
    }

    internal void HandleAttackWindupStarted()
    {
        PlayAttackStart();
    }

    internal void HandleAttackStrikeStarted()
    {
        PlayAttackEnd();
    }

    internal void HandleGrabStarted(Enemy owner, float horizontalOffset)
    {
        if (owner == null)
        {
            return;
        }

        if (Direction2D.IsOpposite(horizontalOffset, owner.FacingDir.x))
        {
            PlayHitBack();
        }
        else
        {
            PlayHitFront();
        }
    }

    private void PlayIdle()
    {
        Play(hashIdle);
    }

    private void PlayWalk()
    {
        Play(hashWalk);
    }

    private void PlayHitFront()
    {
        Play(hashHitFront);
    }

    private void PlayHitBack()
    {
        Play(hashHitBack);
    }

    private void PlayAttackStart()
    {
        Play(hashAttack);
    }

    private void PlayAttackEnd()
    {
        Play(hashAttackEnd);
    }

    private void Play(int stateHash)
    {
        if (animator != null)
        {
            animator.Play(stateHash);
        }
    }

    public void AnimEvent_SpawnAttackEffect()
    {
        if (attackEffectObject != null)
        {
            attackEffectObject.SetActive(true);
        }
    }
}
