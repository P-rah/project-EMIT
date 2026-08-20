using UnityEngine;

public class EnemyOverlapDetector<T> : MonoBehaviour where T : IKnockbackable
{
    public float detectRadius = 1.5f;
    public LayerMask enemyLayer;

    private T owner;

    private bool detect;

    private void Awake()
    {
        owner = GetComponent<T>();
        detect = false;
    }

    private void Update()
    {
        if (!detect) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.gameObject == this.gameObject) continue;

            AttackInfo info = new AttackInfo
            {
                baseDamage = 1,
                power = owner.currentPower,
                knockback = owner.currentKnockBackForce,
                attackIsGrab = false
            };

            if (hit.TryGetComponent<IDamageable>(out var knockbackTarget))
            {
                knockbackTarget.ReceiveAttack(info);
                //if(Mathf.Abs(knockbackTarget.currentPower) < Mathf.Abs(owner.currentPower))
                knockbackTarget.SetPower(owner.currentPower);
            }
        }
    }

    public void startDetect()
    {
        detect = true;
    }

    public void stopDetect()
    {
        detect = false;
    }
}
