using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    /* INTEGRATION:
     * 이 콜백은 이 브랜치에서 추가된 Laser가 EnemyAttackHitbox를 상속해 동일한 플레이어 피격 처리를
     * 재사용할 수 있도록 private이 아닌 protected로 열어 두었다. integration/enemy-state-merge의
     * private 선언으로 되돌리면 파생 공격 히트박스의 확장 지점이 사라지므로 protected를 유지한다.
     */
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();

            //if (player != null)
            //{
            //    // 적이 바라보는 방향(facingDirection)을 그대로 플레이어의 넉백 방향으로 전달!
            //    player.TakeDamage(damage, enemy.facingDirection * knockbackPower);
            //}
            player.PlayerDead();
            Debug.Log("Player Hit");
        }
    }

    public void AnimEvent_DisableEffect()
    {
        /* INTEGRATION:
         * 공격 상태의 소유권은 Enemy가 아니라 EnemyStateMachine으로 이동했다. 애니메이션 이벤트가
         * 공격 종료 후 취소 가능 플래그를 복구하는 로직이므로, 통합 시 기존 enemy.AttackState 접근은
         * 사용하지 말고 아래 StateMachine 경로를 유지한다.
         */
        enemy.StateMachine.AttackState.SetAttackCancellable();
        gameObject.SetActive(false);
    }
}
