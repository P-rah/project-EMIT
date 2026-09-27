//using System.Collections;
//using UnityEngine;

//public class BossAttackState : StateBase<BossStateMachine>
//{
//    private Boss1 boss;

//    private const int LaserCount = 5;
//    private const float attackInterval = 2f;
//    private const float AttackDuration = 2f;

//    private Coroutine attackCoroutine;

//    public BossAttackState(BossStateMachine owner) : base(owner)
//    {
//        boss = owner.boss;
//    }

//    public override void EnterState()
//    {
//        boss.PrepareHovering();
//        attackCoroutine = boss.StartCoroutine(AttackRoutine());
//    }

//    public override void ExitState()
//    {
//        if (attackCoroutine != null)
//        {
//            boss.StopCoroutine(attackCoroutine);
//            attackCoroutine = null;
//        }
//        boss.laserAttack?.CancelTargeting();
//    }

//    private IEnumerator AttackRoutine()
//    {
//        for (int i = 0; i < LaserCount; i++)
//        {
//            boss.StartTargetting();
//            if (i < LaserCount - 1)
//            {
//                yield return new WaitForSeconds(attackInterval);
//            }
//        }

//        if (LaserCount > 0)
//        {
//            yield return new WaitForSeconds(AttackDuration);
//        }

//        attackCoroutine = null;

//        if (owner.IsCurrentState(this))
//        {
//            owner.ChangeState(owner.RecoilState);
//        }
//    }

//    //슈퍼아머
//    public override bool CanBeCancelled()
//    {
//        return false;
//    }
//}
