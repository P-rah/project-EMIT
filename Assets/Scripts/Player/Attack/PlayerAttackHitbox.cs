using UnityEngine;
using System;

public class PlayerAttackHitbox : MonoBehaviour
{
    public event Action<Transform, Vector2> OnTargetSweepHit;

    [Header("Knockback Settings")]
    public float knockbackPower = 10f;
    public Vector2 localKnockbackDir = new Vector2(1, 0);
    private Vector2 finalForce;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            OnTargetSweepHit?.Invoke(other.transform, hitPoint);

            float facingDirection = transform.root.right.x > 0 ? 1f : -1f;

            Vector2 finalKnockbackDirection = new Vector2(
                localKnockbackDir.x * facingDirection,
                localKnockbackDir.y
                ).normalized;
            finalForce = finalKnockbackDirection * knockbackPower;

            AttackInfo info = new AttackInfo
            {
                knockbackForce = finalForce,
                attackIsGrab = false
            };

            target.ReceiveAttack(info);
        }
    }
}
