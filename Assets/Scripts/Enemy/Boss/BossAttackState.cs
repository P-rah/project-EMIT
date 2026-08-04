using System.Collections;
using UnityEngine;

public class BossAttackState : BossState
{
    private const int LaserCount = 5;
    private const float LaserSpawnInterval = 1f;
    private const float AttackDuration = 5f;

    private Coroutine attackCoroutine;

    public BossAttackState(BossStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        boss.PrepareHovering();
        attackCoroutine = boss.StartCoroutine(AttackRoutine());
    }

    public override void ExitState()
    {
        if (attackCoroutine != null)
        {
            boss.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        for (int i = 0; i < LaserCount; i++)
        {
            boss.FireLaser();

            if (i < LaserCount - 1)
            {
                yield return new WaitForSeconds(LaserSpawnInterval);
            }
        }

        if (LaserCount > 0)
        {
            yield return new WaitForSeconds(AttackDuration);
        }

        attackCoroutine = null;

        if (stateMachine.IsCurrentState(this))
        {
            stateMachine.ChangeState(stateMachine.RecoilState);
        }
    }
}
