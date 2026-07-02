using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * 플레이어 공격, 역행 담당 스크립트
 */

public class PlayerCombatController : MonoBehaviour
{
    private bool isAttacking = false;
    private bool attackable = true;
    private bool attackReverse = false; //공격 애니메이션 정방향, 역방향 출력을 위함

    private bool isEmiting = false;
    private bool emitable = true;

    [Header("Attack Settings")]
    public LayerMask enemyLayers;
    public float attackCooldown = 1f;
    public float attackContinuousCooldown = 0.2f;

    public Transform attackSweepPoint;
    public float attackSweepRange = 0.5f;

    public Transform attackGrabPoint;
    public float attackGrabRange = 0.1f;
    public float attackGrabDistance = 0.5f;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;
    public EmitShaderController _emitShaderController;

    public GameplayCameraController _gameplayCameraController;

    public void OnAttackSweep(InputAction.CallbackContext context)
    {
        if (attackable)
        {
            StartCoroutine(AttackSweepRoutine());
        }
    }

    public void OnAttackGrab(InputAction.CallbackContext context)
    {
        if (attackable)
        {
            StartCoroutine(AttackGrabRoutine());
        }
    }

    public void OnEmit(InputAction.CallbackContext context)
    {
        if (emitable)
        {
            StartCoroutine(EmitRoutine());
        }

    }

    private IEnumerator AttackSweepRoutine()
    {
        attackable = false;
        isAttacking = true;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackSweepPoint.position, attackSweepRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            _gameplayCameraController.CameraShake(2f, 1f);
            // enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }

        isAttacking = false;
        attackReverse = true;

        yield return new WaitForSeconds(attackCooldown);
        attackable = true;

        yield return new WaitForSeconds(attackContinuousCooldown);
        attackReverse = false;
    }

    private IEnumerator AttackGrabRoutine()
    {
        attackable = false;
        isAttacking = true;
        Vector2 direction = transform.right;
        RaycastHit2D hitInfo = Physics2D.CircleCast(attackGrabPoint.position, attackGrabRange, direction, attackGrabDistance, enemyLayers);
        if (hitInfo.collider != null)
        {
            Debug.Log(hitInfo.collider.name + "hit!");

            // Enemy enemy = hitInfo.collider.GetComponent<Enemy>();
            // if (enemy != null)
            // {
            //     enemy.TakeDamage(attackDamage);
            // }
        }
        isAttacking = false;
        attackReverse = true;

        yield return new WaitForSeconds(attackCooldown);
        attackable = true;

        yield return new WaitForSeconds(attackContinuousCooldown);
        attackReverse = false;
    }

    private IEnumerator EmitRoutine()
    {
        emitable = false;
        isEmiting = !isEmiting;

        if (isEmiting)
        {
            _emitShaderController.StartEffect();
            Debug.Log("Start");
        }
        else
        {
            _emitShaderController.EndEffect();
            Debug.Log("End");
        }
        yield return new WaitForSeconds(emitCooldown);
        emitable = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackSweepPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackSweepPoint.position, attackSweepRange);

        }
        if (attackGrabPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackGrabPoint.position, attackGrabRange);
            Gizmos.DrawLine(attackGrabPoint.position, attackGrabPoint.position + transform.right * attackGrabDistance);
            Gizmos.DrawWireSphere(attackGrabPoint.position + transform.right * attackGrabDistance, attackGrabRange);
        }
    }

}
