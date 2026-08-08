using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(EnemyStateMachine owner) : base(owner)
    {
    }

    private Coroutine attackCoroutine;
    private bool isCancellable = false;

    public override void EnterState()
    {
        enemy.RootMovement();
        TryAttack();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (!stateMachine.IsCurrentState(this) || IsAttacking())
        {
            return;
        }
        enemy.Flip();
        if (EmitManager.Instance.isEmitting)
        {
            return;
        }
        TryAttack();
    }

    public override void ExitState()
    {
        /* INTEGRATION:
         * integration/enemy-state-merge의 공격 외곽선 초기화는 이 메서드 시작부에 추가하되, 아래 코루틴
         * 정리와 이동 잠금 해제는 그대로 둔다. 피격으로 선딜레이가 중단되는 경로에서도 두 정리가 모두
         * 실행되어야 공격 외곽선과 Root 상태가 남지 않는다.
         */
        enemy.outlineController?.SetDefaultOutline();
        if (attackCoroutine != null)
        {
            enemy.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            enemy.UnrootMovement();
            enemy.canChangeState = true;
        }
    }

    public void TryAttack()
    {   
        enemy.canChangeState = false;
        attackCoroutine = enemy.StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isCancellable = false;
        enemy.NotifyAttackWindupStarted();
        if (enemy.myAudioSource != null && enemy.attackReadySound != null)
        {
            enemy.myAudioSource.PlayOneShot(enemy.attackReadySound);
        }
        enemy.outlineController?.SetAttackOutline();

        yield return new WaitForSeconds(enemy.startUpTime);

        // 본공격
        enemy.NotifyAttackStrikeStarted();
        CameraShake.Instance.ShakeCamera(10f,0.2f);
        if (enemy.myAudioSource != null && enemy.attackSound != null)
        {
            enemy.myAudioSource.PlayOneShot(enemy.attackSound);
        }

        // 후딜레이 시간
        yield return new WaitForSeconds(enemy.recoveryTime);
        attackCoroutine = null;
        enemy.UnrootMovement();
        enemy.canChangeState = true;
    }

    public bool IsAttacking()
    {
        return attackCoroutine != null;
    }

    public override bool CanBeCancelled()
    {
        return isCancellable;
    }

    public void SetCancellable(bool value)
    {
        isCancellable = value;
    }
}
