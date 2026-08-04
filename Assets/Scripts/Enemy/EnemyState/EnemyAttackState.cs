using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    /* INTEGRATION:
     * 상태 생성자에는 Enemy와 EnemyAnimator를 따로 전달하지 않고 EnemyStateMachine 하나만 전달한다.
     * StateMachine이 상태 인스턴스의 생성/전환을 단일 책임으로 맡고, Enemy와 Animator는 Owner를 통해
     * 얻도록 한 리팩터링이므로 integration/enemy-state-merge의 기존 생성자 형태로 되돌리지 않는다.
     */
    public EnemyAttackState(EnemyStateMachine stateMachine) : base(stateMachine) { }
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
        // 선딜레이 시간
        /* INTEGRATION:
         * 선딜레이 중 피격으로 공격을 끊을 수 있게 하려고 isCancellable을 true로 둔다. 통합 브랜치의
         * false를 선택하면 경고 동작 중 넉백/그랩을 받아도 공격 코루틴이 계속된다. 따라서 true를 유지하고
         * 공격 외곽선 setAtrackOutline 호출만 PlayAttackStart 직후에 결합한다. 본공격 시작 시 false로 바뀌는
         * 현재 경계는 유지한다.
         */
        isCancellable = true;
        enemy.enemyAnim.PlayAttackStart();
        enemy.myAudioSource.PlayOneShot(enemy.attackReadySound);

        yield return new WaitForSeconds(enemy.startUpTime);
        // 선딜레이 시간 동안에 넉백을 받았다면 TryAttack이 실패하고 KnockBackState가 되어야 함

        // 본공격
        isCancellable = false;
        enemy.enemyAnim.PlayAttackEnd();
        CameraShake.Instance.ShakeCamera(10f,0.2f);
        enemy.myAudioSource.PlayOneShot(enemy.attackSound);

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

    public bool CanBeCancelled()
    {
        return isCancellable;
    }

    public void SetAttackCancellable()
    {
        isCancellable = true;
    }
}
