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
    private readonly int hashAttack = Animator.StringToHash("EnemyAttack");
    private readonly int hashAttackEnd = Animator.StringToHash("EnemyAttackEnd");

    public void StopAnimControl()
    {
        animator.enabled = false;
    }
    public void StartAnimControl()
    {
        animator.enabled = true;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        StartAnimControl();
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
        animator.Play(hashAttack);
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
}
