using UnityEngine;
using System;
using EMIT.Utilities;

public class PlayerAttackHitbox : MonoBehaviour
{
    private const int AttackPower = 1;

    public event Action<Transform, Vector2> OnTargetSweepHit;

    [Header("Knockback Settings")]
    public float knockbackPower = 10f;
    private Vector2 knockbackVec;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target == null) return;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        OnTargetSweepHit?.Invoke(other.transform, hitPoint);

        Vector2 facingDir = Direction2D.FacingDir(transform.root);
        knockbackVec = facingDir * knockbackPower;

        AttackInfo info = new AttackInfo
        {
            baseDamage = 1,
            power = (int)facingDir.x * AttackPower,
            knockback = knockbackVec,
            attackIsGrab = false
        };

        target.ReceiveAttack(info);
    }
}
