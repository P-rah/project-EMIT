using UnityEngine;

public class EnemyDeadState : EnemyState
{
    /* INTEGRATION:
     * 다른 EnemyState와 동일하게 상태 생성/소유권을 EnemyStateMachine에 모은 생성자다. 통합 시
     * (Enemy, EnemyAnimator) 생성자로 되돌리지 말고 integration 브랜치의 사망 연출만 EnterState에 합친다.
     */
    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) {}

    public override void EnterState()
    {
        Debug.Log("Enemy is Dead");
        /* INTEGRATION:
         * 사망 파편의 좌우 방향을 확정하기 위해 먼저 Enemy의 논리 방향과 회전을 동기화한다. 현재 브랜치의
         * EffectManager에는 방향 인자가 없어 호출을 비활성화했지만 integration/enemy-state-merge에는
         * PlayDeathEffect(position, isFacingLeft)가 있으므로 병합 후 바로 다음 줄의 호출을 활성화한다.
         * Emit 가능 여부는 통합 브랜치에서도 PlayerCombat의 적중 카운트가 관리하므로 여기서 별도로
         * isEmittable을 true로 설정하지 않는다.
         */
        enemy.Flip();
        EffectManager.Instance?.PlayDeathEffect(enemy.transform.position, enemy.isFacingLeft);
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        // 적이 죽은 이후 실행시킬 로직

    }

    public override void CheckState()
    {

    }
}
