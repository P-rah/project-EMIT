using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler), typeof(PlayerState))]
public class PlayerCombat : MonoBehaviour
{
    private PlayerInputHandler input;
    private PlayerState state;

    [Header("Attack Settings")]
    public LayerMask enemyLayers;
    public float attackJudgeTime = 0.2f;
    public float attackMotionLimit = 0.3f;

    public Transform attackSweepPoint;
    public float attackSweepRange = 0.5f;

    public Transform attackGrabPoint;
    public float attackGrabRange = 0.1f;
    public float attackGrabDistance = 8f;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;

    private bool canAttack = true;
    private bool canEmit = true;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = GetComponent<PlayerState>();
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnAttackSweepPressed", ExecuteAttackSweep);
        EventManager.StartListening("OnAttackGrapPressed", ExecuteAttackGrap);
        EventManager.StartListening("OnEmitPressed", ExecuteEmit);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnAttackSweepPressed", ExecuteAttackSweep);
        EventManager.StopListening("OnAttackGrapPressed", ExecuteAttackGrap);
        EventManager.StopListening("OnEmitPressed", ExecuteEmit);
    }

    public void ExecuteAttackSweep()
    {
        if (canAttack)
            StartCoroutine(AttackSweepRoutine());
    }

    public void ExecuteAttackGrap()
    {
        if (canAttack)
            StartCoroutine(AttackGrapRoutine());
    }

    public void ExecuteEmit()
    {
        if (canEmit)
            StartCoroutine(EmitRoutine());
    }

    private IEnumerator AttackSweepRoutine()
    {
        canAttack = false;
        state.isAttackingGrap = false;
        state.isAttackingSweep = true;

        //공격 이펙트 활성화
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackSweepPoint.position, attackSweepRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log(enemy.name + "hit!");
            CameraShake.Instance.ShakeCamera(5f, 0.5f);
            // enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackJudgeTime);
        //공격 이펙트 비활성화
        state.attacked = true;
        canAttack = true;

        yield return new WaitForSeconds(attackMotionLimit);
        //공격 모션 종료
        state.isAttackingSweep = false;
        state.attacked = false;
    }

    private IEnumerator AttackGrapRoutine()
    {
        canAttack = false;
        state.isAttackingSweep = false;
        state.isAttackingGrap = true;

        Vector2 direction = transform.right;
        RaycastHit2D hitInfo = Physics2D.CircleCast(attackGrabPoint.position, attackGrabRange, direction, attackGrabDistance, enemyLayers);
        if (hitInfo.collider != null)
        {
            Debug.Log(hitInfo.collider.name + "hit!");
            CameraShake.Instance.ShakeCamera(5f, 0.5f);
            // Enemy enemy = hitInfo.collider.GetComponent<Enemy>();
            // if (enemy != null)
            // {
            //     enemy.TakeDamage(attackDamage);
            // }
        }

        yield return new WaitForSeconds(attackJudgeTime);
        //공격 이펙트 비활성화
        state.attacked = true;
        canAttack = true;

        yield return new WaitForSeconds(attackMotionLimit);
        //공격 모션 종료
        state.isAttackingGrap = false;
        state.attacked = false;
    }

    private IEnumerator EmitRoutine()
    {
        canEmit= false;
        state.isEmiting = !state.isEmiting;

        yield return new WaitForSeconds(emitCooldown);
        canEmit = true;
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
