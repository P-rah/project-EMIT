using System;
using EMIT.Utilities;
using UnityEngine;

public class PlayerGrapHitbox : MonoBehaviour
{
    /* INTEGRATION:
     * 이 브랜치는 공격 방향 계산을 Direction2D로 통일하고 AttackInfo의 벡터 필드명을 knockback으로
     * 변경했다. integration/enemy-state-merge의 knockbackForce/GetFacingDirection 구현을 섞으면
     * IDamageable 계약과 맞지 않아 컴파일되지 않으므로 아래 AttackInfo 생성 방식을 유지한다.
     * power는 현재 '공격자의 월드 바라보기 방향' 규약이고 실제 당기기 동작은 attackIsGrab이 구분한다.
     * 밀기/당기기의 물리 방향을 power 부호로 표현하는 설계로 바꿀 경우 이 값만 음수화하지 말고
     * PlayerAttackHitbox, DamageCalculation, Boss1의 해석을 동시에 변경하고 연계 피해를 재검증한다.
     */
    private const int AttackPower = 1;

    public event Action<Transform, Vector2> OnTargetGrapHit;

    [Header("히트박스 설정")]
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

    //private int GetFacingDirection()
    //{
    //    return transform.root.right.x >= 0f ? 1 : -1;
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, transform.right * attackDistance);
        Gizmos.DrawWireCube(transform.position + transform.right * (attackDistance / 2f), new Vector3(attackDistance, boxSize.y, 0));
    }
}
