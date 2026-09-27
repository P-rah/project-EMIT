using System.Collections.Generic;
using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    private Animator animator;
    private Boss_R1 bossEnemy;

    private Dictionary<string, GameObject> childEffects = new Dictionary<string, GameObject>();

    private readonly int hashIdle = Animator.StringToHash("EnemyIdle");
    private readonly int hashWalk = Animator.StringToHash("EnemyWalk");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossEnemy = GetComponent<Boss_R1>();

        CacheChildEffects();

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

    internal void HandleStateEntered(IEnemyStateMachine stateMachine, IState state, Vector2 knockBackForce, Vector2 facingDir)
    {
        switch (state)
        {
            case var _ when state == stateMachine.IdleState:
                PlayIdle();
                break;

            case var _ when state == stateMachine.ChaseState:
                PlayWalk();
                break;
        }
    }

    internal void HandlePlayAnimation(string animName = null)
    {
        if (string.IsNullOrEmpty(animName))
        {
            Debug.Log("존재하지 않는 애니메이션");
            return;
        }
        else
        {
            Play(Animator.StringToHash(animName));
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

    private void Play(int stateHash)
    {
        if (animator != null)
        {
            animator.Play(stateHash, -1, 0f);
        }
    }

    private void CacheChildEffects()
    {
        Transform[] allTransforms = GetComponentsInChildren<Transform>(true);
        foreach (Transform tf in allTransforms)
        {
            if (!childEffects.ContainsKey(tf.name))
            {
                childEffects.Add(tf.name, tf.gameObject);
            }
        }
    }

    public void AnimEvent_SpawnAttackEffect()
    {
        if (bossEnemy != null && bossEnemy.CurrentStrategy != null)
        {
            string effectName = bossEnemy.CurrentStrategy.childEffectName;

            if (!string.IsNullOrEmpty(effectName) && childEffects.TryGetValue(effectName, out GameObject effectObj))
            {
                effectObj.SetActive(false);
                effectObj.SetActive(true);
                return;
            }
        }
    }
}
