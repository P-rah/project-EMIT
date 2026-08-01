using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
        enemy.StateMachine.AttackState.SetAttackCancellable();
        gameObject.SetActive(false);
    }
}
