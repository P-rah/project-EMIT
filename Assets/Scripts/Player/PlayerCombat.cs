using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class PlayerCombat : MonoBehaviour
{
    private PlayerState state;

    [Header("Hitbox Colliders")]
    public GameObject sweepCollider;
    public GameObject grapCollider;

    [Header("Attack Settings")]
    public float attackingSweepTime = 0.2f;
    public float attackingGrapTime = 0.5f;
    public float attackCoolTime = 0.3f;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;

    private bool canAttack = true;
    private bool canEmit = true;

    private void Awake()
    {
        state = Player.State;
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnEmitPressed", ExecuteEmit);
    }

    private void OnDisable()
    {
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
        state.isAttackingSweep = true;

        //공격 처리
        sweepCollider.SetActive(true);

        yield return new WaitForSeconds(attackingSweepTime);
        sweepCollider.SetActive(false);
        state.isAttackingSweep = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttack = true;
    }

    private IEnumerator AttackGrapRoutine()
    {
        canAttack = false;
        state.isAttackingGrap = true;

        //공격 처리
        grapCollider.SetActive(true);

        yield return new WaitForSeconds(attackingGrapTime);
        grapCollider.SetActive(false);
        state.isAttackingGrap = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttack = true;
    }

    private IEnumerator EmitRoutine()
    {
        canEmit= false;
        state.isEmiting = !state.isEmiting;

        yield return new WaitForSeconds(emitCooldown);
        canEmit = true;
    }
}
