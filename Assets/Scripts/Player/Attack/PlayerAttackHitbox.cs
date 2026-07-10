using UnityEngine;
using System;

public class PlayerAttackHitbox : MonoBehaviour
{
    public event Action<Transform, Vector2> OnTargetHit;

    [Header("Knockback Settings")]
    public float knockbackPower = 10f;
    public Vector2 localKnockbackDir = new Vector2(1, 0);
    private Vector2 finalForce = Vector2.zero;

    public bool attackIsGrab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            OnTargetHit?.Invoke(other.transform, hitPoint);

            if (!attackIsGrab)
            {
                float facingDirection = transform.root.right.x > 0 ? 1f : -1f;

                Vector2 finalKnockbackDirection = new Vector2(
                    localKnockbackDir.x * facingDirection,
                    localKnockbackDir.y
                    ).normalized;
                finalForce = finalKnockbackDirection * knockbackPower;
            }

            AttackInfo info = new AttackInfo
                {
                    knockbackForce = finalForce,
                    attackIsGrab = attackIsGrab
                };

            target.ReceiveAttack(info);
        }
    }
}
