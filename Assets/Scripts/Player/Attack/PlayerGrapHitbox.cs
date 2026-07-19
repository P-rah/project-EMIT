using System;
using UnityEngine;

public class PlayerGrapHitbox : MonoBehaviour
{
    public event Action<Transform, Vector2> OnTargetGrapHit;

    [Header("타격 범위 설정")]
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

            if (target != null)
            {
                float facingDirection = transform.root.right.x > 0 ? 1f : -1f;
                OnTargetGrapHit?.Invoke(hit.collider.transform, hit.point);
                Debug.Log(target);

                AttackInfo info = new AttackInfo
                {
                    knockbackForce = new Vector2(facingDirection, 0f),
                    attackIsGrab = true
                };
                target.ReceiveAttack(info);
                break;
            }
            else
            {
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, transform.right * attackDistance);
        Gizmos.DrawWireCube(transform.position + transform.right * (attackDistance / 2f), new Vector3(attackDistance, boxSize.y, 0));
    }
}
