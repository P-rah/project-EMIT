using UnityEngine;

public class Laser : EnemyAttackHitbox
{
    [Header("Settings")]
    public string poolTag = "BossLaser";
    public float lifeTime = 0.2f;

    private void OnEnable()
    {
        Invoke("Deactivate", lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EventManager.TriggerEvent("OnPlayerHit");
        }
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
    }
}
