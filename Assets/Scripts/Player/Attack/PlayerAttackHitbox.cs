using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackPower = 10f;
    public Vector2 localKnockbackDir = new Vector2(1, 0);

    public bool attackIsGrap;

    private bool hasTriggeredHitEffect = false;

    private void OnEnable()
    {
        hasTriggeredHitEffect = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Attackable target = other.GetComponent<Attackable>();

        if (target != null)
        {
            float facingDirection = transform.root.right.x > 0 ? 1f : -1f;

            Vector2 finalKnockbackDirection = new Vector2(
                localKnockbackDir.x * facingDirection,
                localKnockbackDir.y
                ).normalized;
            Vector2 finalForce = finalKnockbackDirection * knockbackPower;

            AttackInfo info = new AttackInfo
            {
                HitPoint = other.ClosestPoint(transform.position),
                KnockbackForce = finalForce,
                AttackIsGrap = attackIsGrap
            };

            target.ReceiveAttack(info);

            if (!hasTriggeredHitEffect)
            {
                CameraShake.Instance.ShakeCamera(10f, 0.2f);
                hasTriggeredHitEffect = true;
            }
        }
    }
}
