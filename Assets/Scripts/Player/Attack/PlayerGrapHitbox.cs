using System;
using EMIT.Utilities;
using UnityEngine;

public class PlayerGrapHitbox : MonoBehaviour
{
    private const int AttackPower = 1;

    public event Action<Transform, Vector2> OnTargetGrapHit;

    [Header("Ÿ�� ���� ����")]
    public Vector2 boxSize = new Vector2(1f, 1f); 
    public float attackDistance = 5f;

    public void CatchClosestEnemy()
    {
        Vector2 direction = transform.right;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            boxSize,
            transform.eulerAngles.z,
            direction,
            attackDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            
            if (target == null) continue;
            
            Vector2 facingDir = Direction2D.FacingDir(transform.root);
            OnTargetGrapHit?.Invoke(hit.collider.transform, hit.point);
            Debug.Log(target);

            AttackInfo info = new AttackInfo
            {
                knockback = facingDir,
                baseDamage = 1,
                power = (int) facingDir.x * AttackPower,
                attackIsGrab = true
            };
            target.ReceiveAttack(info);
            break;
        }
    }

    private int GetFacingDirection()
    {
        return transform.root.right.x >= 0f ? 1 : -1;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, transform.right * attackDistance);
        Gizmos.DrawWireCube(transform.position + transform.right * (attackDistance / 2f), new Vector3(attackDistance, boxSize.y, 0));
    }
}
