using UnityEngine;
using System;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private GameObject attackEffectObject;

    private readonly int hashIdle = Animator.StringToHash("EnemyIdle");
    private readonly int hashWalk = Animator.StringToHash("EnemyWalk");
    private readonly int hashHitFront = Animator.StringToHash("EnemyHitFront");
    private readonly int hashHitBack = Animator.StringToHash("EnemyHitBack");
    private readonly int hashAttackStart = Animator.StringToHash("EnemyAttackStart");
    private readonly int hashAttackEnd = Animator.StringToHash("EnemyAttackEnd");

    public void stopAnimControll()
    {
        animator.enabled = false;
    }
    public void startAnimControll()
    {
        animator.enabled = true;
    }

    //public event Action OnAttackFrame;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        startAnimControll();
    }

    public void PlayIdle()
    {
        animator.Play(hashIdle);
    }

    public void PlayWalk()
    {
         animator.Play(hashWalk);
    }

    public void PlayHitFront()
    {
        animator.Play(hashHitFront);
    }

    public void PlayHitBack()
    {
        animator.Play(hashHitBack);
    }

    public void PlayAttackStart()
    {
        animator.Play(hashAttackStart);
    }

    public void PlayAttackEnd()
    {
        animator.Play(hashAttackEnd);
    }

    public void AnimEvent_SpawnAttackEffect()
    {
        if (attackEffectObject != null)
        {
            attackEffectObject.SetActive(true);
        }
    }

    //public void AnimEvent_TriggerAttack()
    //{
    //    OnAttackFrame?.Invoke();
    //}
}
