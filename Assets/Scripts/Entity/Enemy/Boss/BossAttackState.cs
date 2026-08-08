using System.Collections;
using UnityEngine;

public class BossAttackState : StateBase<BossStateMachine>
{
    private Boss1 boss;

    private const int LaserCount = 5;
    private const float LaserSpawnInterval = 1f;
    private const float AttackDuration = 4f;

    private Coroutine attackCoroutine;

    public BossAttackState(BossStateMachine owner) : base(owner)
    {
        boss = owner.boss;
    }

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

        if (owner.IsCurrentState(this))
        {
            owner.ChangeState(owner.RecoilState);
        }
    }

    //슈퍼아머
    public override bool CanBeCancelled()
    {
        return false;
    }
}
