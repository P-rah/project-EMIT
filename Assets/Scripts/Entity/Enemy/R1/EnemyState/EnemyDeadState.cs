using UnityEngine;

public class EnemyDeadState : EnemyState
{
    /* INTEGRATION:
     * 다른 EnemyState와 동일하게 상태 생성/소유권을 EnemyStateMachine에 모은 생성자다. 통합 시
     * (Enemy, EnemyAnimator) 생성자로 되돌리지 말고 integration 브랜치의 사망 연출만 EnterState에 합친다.
     */
    private DeathEffectType type;

    public EnemyDeadState(EnemyStateMachine owner, DeathEffectType type) : base(owner)
    {
        this.type = type;
    }

    public override void EnterState()
    {
        Debug.Log("Enemy is Dead");

        enemy.Flip();
        // 기존 사망 풀 호출은 보존하되 사용하지 않는다.
        // EffectManager.Instance?.PlayDeathEffect(type, enemy.transform.position, enemy.isFacingLeft);
        if (enemy.deathFragmentPrefab != null)
        {
            // 부모 없이 생성하여 적 본체가 삭제되어도 파편 연출은 계속된다.
            Object.Instantiate(enemy.deathFragmentPrefab, enemy.transform.position,
                Quaternion.Euler(0f, enemy.isFacingLeft ? 180f : 0f, 0f));
        }
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        // 사망은 최종 상태이므로 이곳에서 어떤 작업도 실행해서는 안 된다.
        // Destroy는 ExitState를 호출하지 않는다. 연출은 EnterState, 정리는 공통 사망 처리에서 수행한다.
    }
}
